using System.Text.Json;
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

        var emissions = await _context.Emissions
            .Where(e => e.PlaylistUrl != null && e.PlaylistUrl != "")
            .ToListAsync(cancellationToken);

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
        var payload = await JsonSerializer.DeserializeAsync<PlaylistItemsResponse>(stream, cancellationToken: cancellationToken);

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

        var playlistId = QueryHelpers.ParseQuery(uri.Query).TryGetValue("list", out var values)
            ? values.FirstOrDefault()
            : null;
        return string.IsNullOrWhiteSpace(playlistId) ? null : playlistId;
    }

    private sealed record LatestVideo(string VideoId);

    private sealed class PlaylistItemsResponse
    {
        public List<PlaylistItem> Items { get; set; } = new();
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