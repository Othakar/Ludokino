using Ludokino.Api.Services.Interfaces;

namespace Ludokino.Api.Services;

public sealed class YoutubePlaylistSyncHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<YoutubePlaylistSyncHostedService> _logger;

    public YoutubePlaylistSyncHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<YoutubePlaylistSyncHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SynchronizeOnceAsync(stoppingToken);

        var intervalMinutes = _configuration.GetValue("Youtube:SyncIntervalMinutes", 60);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(5, intervalMinutes)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SynchronizeOnceAsync(stoppingToken);
        }
    }

    private async Task SynchronizeOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IYoutubePlaylistSyncService>();
            var count = await service.SynchronizeAsync(cancellationToken);
            if (count > 0) _logger.LogInformation("{Count} émission(s) synchronisée(s) depuis YouTube.", count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erreur pendant la synchronisation des playlists YouTube.");
        }
    }
}