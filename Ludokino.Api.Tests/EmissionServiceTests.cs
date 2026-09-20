using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Ludokino.Api.Models;
using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.Services;
using Xunit;

namespace Ludokino.Api.Tests;

public class EmissionServiceTests
{
    [Fact]
    public async Task CreateAsync_WithoutYoutubeUrl_ThrowsArgumentException()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new EmissionService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateEmissionRequest
        {
            Name = "Ludokino Show"
        }));
    }

    [Fact]
    public async Task CreateAsync_WithNonYoutubeUrl_ThrowsArgumentException()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new EmissionService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateEmissionRequest
        {
            Name = "Ludokino Show",
            YoutubeUrl = "https://example.com/video"
        }));
    }

    [Fact]
    public async Task CreateAsync_WithYoutubeUrl_ReturnsYoutubeUrl()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new EmissionService(context);

        var emission = await service.CreateAsync(new CreateEmissionRequest
        {
            Name = "Ludokino Show",
            YoutubeUrl = "https://youtu.be/example"
        });

        Assert.Equal("https://youtu.be/example", emission.YoutubeUrl);
    }

    [Fact]
    public async Task SynchronizeAsync_StoresThumbnailWhenItIsMissing()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Emissions.Add(new Emission
        {
            Name = "Ludokino Show",
            Slug = "ludokino-show",
            YoutubeUrl = "https://www.youtube.com/playlist?list=playlist-1",
            PlaylistUrl = "https://www.youtube.com/playlist?list=playlist-1"
        });
        await context.SaveChangesAsync();

        var service = CreateYoutubeSyncService(context, new YoutubeApiHandler("video-1"));

        var synchronizedCount = await service.SynchronizeAsync();

        var emission = Assert.Single(context.Emissions);
        Assert.Equal(1, synchronizedCount);
        Assert.Equal("https://i.ytimg.com/vi/video-1/maxresdefault.jpg", emission.ThumbnailUrl);
        Assert.Equal("video-1", emission.LatestVideoId);
    }

    [Fact]
    public async Task SynchronizeAsync_ReplacesWhitespaceThumbnail()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        context.Emissions.Add(new Emission
        {
            Name = "Ludokino Show",
            Slug = "ludokino-show",
            YoutubeUrl = "https://www.youtube.com/playlist?list=playlist-1",
            PlaylistUrl = "https://www.youtube.com/playlist?list=playlist-1",
            ThumbnailUrl = "   "
        });
        await context.SaveChangesAsync();

        await CreateYoutubeSyncService(context, new YoutubeApiHandler("video-whitespace")).SynchronizeAsync();

        Assert.Equal("https://i.ytimg.com/vi/video-whitespace/maxresdefault.jpg", context.Emissions.Single().ThumbnailUrl);
    }

    [Fact]
    public async Task SynchronizeAsync_PreservesExistingThumbnailWhenLatestVideoChanges()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = TestDbContextFactory.CreateInMemory(databaseName);
        context.Emissions.Add(new Emission
        {
            Name = "Ludokino Show",
            Slug = "ludokino-show",
            YoutubeUrl = "https://www.youtube.com/playlist?list=playlist-1",
            PlaylistUrl = "https://www.youtube.com/playlist?list=playlist-1",
            ThumbnailUrl = "https://cdn.example.com/published.jpg",
            LastSyncedAt = DateTime.UtcNow.AddHours(-1)
        });
        await context.SaveChangesAsync();

        var service = CreateYoutubeSyncService(context, new YoutubeApiHandler("video-2"));

        await service.SynchronizeAsync();

        var emission = Assert.Single(context.Emissions);
        Assert.Equal("video-2", emission.LatestVideoId);
        Assert.Equal("https://cdn.example.com/published.jpg", emission.ThumbnailUrl);

        await using var reloadedContext = TestDbContextFactory.CreateInMemory(databaseName);
        var persisted = await reloadedContext.Emissions.SingleAsync();
        Assert.Equal("https://cdn.example.com/published.jpg", persisted.ThumbnailUrl);
        Assert.Equal("video-2", persisted.LatestVideoId);
    }

    private static YoutubePlaylistSyncService CreateYoutubeSyncService(
        Ludokino.Api.Data.AppDbContext context,
        HttpMessageHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Youtube:ApiKey"] = "test-key",
                ["Youtube:ChannelId"] = "channel-1",
                ["Youtube:MinimumSyncIntervalMinutes"] = "5"
            })
            .Build();

        return new YoutubePlaylistSyncService(
            context,
            new HttpClient(handler),
            configuration,
            new LoggerFactory().CreateLogger<YoutubePlaylistSyncService>());
    }

    private sealed class YoutubeApiHandler(string videoId) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = request.RequestUri?.AbsolutePath.Contains("playlists", StringComparison.Ordinal) == true
                ? "{\"items\":[],\"nextPageToken\":null}"
                : $"{{\"items\":[{{\"snippet\":{{\"publishedAt\":\"2026-09-20T10:00:00Z\",\"resourceId\":{{\"videoId\":\"{videoId}\"}}}}}}]}}";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}