using System.Text.Json;
using System.Text.RegularExpressions;
using Ludokino.Api.Data;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;

namespace Ludokino.Api.Services;

public class YoutubePlaylistSyncService : IYoutubePlaylistSyncService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<YoutubePlaylistSyncService> _logger;

    public YoutubePlaylistSyncService(
        AppDbContext context,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<YoutubePlaylistSyncService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<int> SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Youtube:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("Synchronisation YouTube ignorée : Youtube:ApiKey n'est pas configurée.");
            return 0;
        }

        var minimumIntervalMinutes = Math.Max(
            5,
            _configuration.GetValue("Youtube:MinimumSyncIntervalMinutes", 30));
        var lastSync = await _context.Emissions
            .Where(emission => emission.LastSyncedAt.HasValue)
            .MaxAsync(emission => (DateTime?)emission.LastSyncedAt, cancellationToken);

        if (lastSync.HasValue && DateTime.UtcNow - lastSync.Value < TimeSpan.FromMinutes(minimumIntervalMinutes))
        {
            _logger.LogInformation(
                "Synchronisation YouTube ignorée : dernière synchronisation il y a moins de {Minutes} minutes.",
                minimumIntervalMinutes);
            return 0;
        }

        var channelId = await ResolveChannelIdAsync(apiKey, cancellationToken);
        if (channelId is null)
        {
            _logger.LogWarning("Synchronisation YouTube impossible : chaîne introuvable.");
            return 0;
        }

        await DiscoverPlaylistsAsync(channelId, apiKey, cancellationToken);

        var emissions = await _context.Emissions
            .Where(e => e.PlaylistUrl != null && e.PlaylistUrl != "")
            .ToListAsync(cancellationToken);

        _logger.LogInformation("{Count} émission(s) avec playlist à synchroniser.", emissions.Count);

        var synchronizedCount = 0;
        foreach (var emission in emissions)
        {
            var playlistId = ExtractPlaylistId(emission.PlaylistUrl);
            if (playlistId is null)
            {
                _logger.LogWarning("Playlist YouTube invalide pour l'émission {EmissionId}.", emission.Id);
                continue;
            }

            var latestVideo = await GetLatestVideoAsync(playlistId, apiKey, cancellationToken);
            if (latestVideo is null) continue;

            emission.YoutubePlaylistId = playlistId;
            emission.LatestVideoId = latestVideo.VideoId;
            emission.YoutubeUrl = $"https://www.youtube.com/watch?v={latestVideo.VideoId}";
            emission.ThumbnailUrl = $"https://i.ytimg.com/vi/{latestVideo.VideoId}/maxresdefault.jpg";
            emission.LastSyncedAt = DateTime.UtcNow;
            emission.UpdatedAt = DateTime.UtcNow;
            synchronizedCount++;
        }

        if (synchronizedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return synchronizedCount;
    }

    private async Task<string?> ResolveChannelIdAsync(string apiKey, CancellationToken cancellationToken)
    {
        var configuredChannelId = _configuration["Youtube:ChannelId"];
        if (!string.IsNullOrWhiteSpace(configuredChannelId)) return configuredChannelId;

        var handle = _configuration["Youtube:ChannelHandle"] ?? "@LDKino";
        var requestUrl = $"https://www.googleapis.com/youtube/v3/channels?part=id&forHandle={Uri.EscapeDataString(handle)}&key={Uri.EscapeDataString(apiKey)}";
        using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ChannelResponse>(stream, JsonOptions, cancellationToken);
        return payload?.Items?.FirstOrDefault()?.Id;
    }

    private async Task DiscoverPlaylistsAsync(string channelId, string apiKey, CancellationToken cancellationToken)
    {
        string? pageToken = null;
        do
        {
            var requestUrl = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet,contentDetails&channelId={Uri.EscapeDataString(channelId)}&maxResults=50&key={Uri.EscapeDataString(apiKey)}";
            if (!string.IsNullOrWhiteSpace(pageToken)) requestUrl += $"&pageToken={Uri.EscapeDataString(pageToken)}";

            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YouTube API a répondu {StatusCode} pendant la découverte des playlists.", response.StatusCode);
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<PlaylistsResponse>(stream, JsonOptions, cancellationToken);
            if (payload?.Items is null) return;

            foreach (var item in payload.Items.Where(item => !string.IsNullOrWhiteSpace(item.Id)))
            {
                await UpsertPlaylistAsync(item, cancellationToken);
            }

            pageToken = payload.NextPageToken;
        }
        while (!string.IsNullOrWhiteSpace(pageToken));

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertPlaylistAsync(PlaylistItemData playlist, CancellationToken cancellationToken)
    {
        var playlistId = playlist.Id!;
        var playlistUrl = $"https://www.youtube.com/playlist?list={playlistId}";
        var emission = await _context.Emissions.FirstOrDefaultAsync(
            item => item.YoutubePlaylistId == playlistId || item.PlaylistUrl == playlistUrl,
            cancellationToken);

        if (emission is null)
        {
            emission = new Models.Emission
            {
                Name = playlist.Snippet?.Title?.Trim() ?? playlistId,
                Slug = await EnsureUniqueSlugAsync(playlist.Snippet?.Title ?? playlistId, cancellationToken),
                Type = "YouTube",
                Description = playlist.Snippet?.Description?.Trim() ?? string.Empty,
                YoutubeUrl = playlistUrl,
                PlaylistUrl = playlistUrl,
                YoutubePlaylistId = playlistId
            };
            _context.Emissions.Add(emission);
        }
        else
        {
            emission.Name = playlist.Snippet?.Title?.Trim() ?? emission.Name;
            emission.Description = playlist.Snippet?.Description?.Trim() ?? emission.Description;
            emission.PlaylistUrl = playlistUrl;
            emission.YoutubePlaylistId = playlistId;
            if (string.IsNullOrWhiteSpace(emission.YoutubeUrl)) emission.YoutubeUrl = playlistUrl;
            emission.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task<string> EnsureUniqueSlugAsync(string value, CancellationToken cancellationToken)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(slug)) slug = "emission";

        var candidate = slug;
        var suffix = 1;
        while (await _context.Emissions.AnyAsync(item => item.Slug == candidate, cancellationToken))
        {
            candidate = $"{slug}-{suffix++}";
        }

        return candidate;
    }

    private async Task<LatestVideo?> GetLatestVideoAsync(string playlistId, string apiKey, CancellationToken cancellationToken)
    {
        var requestUrl = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={Uri.EscapeDataString(playlistId)}&maxResults=50&key={Uri.EscapeDataString(apiKey)}";
        using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("YouTube API a répondu {StatusCode} pour la playlist {PlaylistId}.", response.StatusCode, playlistId);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<PlaylistItemsResponse>(stream, JsonOptions, cancellationToken);

        return payload?.Items?
            .Where(item => !string.IsNullOrWhiteSpace(item.Snippet?.ResourceId?.VideoId))
            .OrderByDescending(item => item.Snippet?.PublishedAt)
            .Select(item => new LatestVideo(item.Snippet!.ResourceId!.VideoId!))
            .FirstOrDefault();
    }

    private static string? ExtractPlaylistId(string? playlistUrl)
    {
        if (!Uri.TryCreate(playlistUrl, UriKind.Absolute, out var uri) || uri.Host is not ("youtube.com" or "www.youtube.com"))
        {
            return null;
        }

        var values = QueryHelpers.ParseQuery(uri.Query);
        if (!values.TryGetValue("list", out var playlistValue))
        {
            return null;
        }

        var playlistId = playlistValue.ToString().Trim();
        return string.IsNullOrWhiteSpace(playlistId) ? null : playlistId;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record LatestVideo(string VideoId);

    private sealed class PlaylistItemsResponse
    {
        public List<PlaylistItem> Items { get; set; } = new();
    }

    private sealed class ChannelResponse
    {
        public List<ChannelItem> Items { get; set; } = new();
    }

    private sealed class ChannelItem
    {
        public string? Id { get; set; }
    }

    private sealed class PlaylistsResponse
    {
        public string? NextPageToken { get; set; }
        public List<PlaylistItemData> Items { get; set; } = new();
    }

    private sealed class PlaylistItemData
    {
        public string? Id { get; set; }
        public PlaylistSnippetData? Snippet { get; set; }
    }

    private sealed class PlaylistSnippetData
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    private sealed class PlaylistItem
    {
        public PlaylistSnippet? Snippet { get; set; }
    }

    private sealed class PlaylistSnippet
    {
        public DateTimeOffset? PublishedAt { get; set; }
        public ResourceId? ResourceId { get; set; }
    }

    private sealed class ResourceId
    {
        public string? VideoId { get; set; }
    }
}