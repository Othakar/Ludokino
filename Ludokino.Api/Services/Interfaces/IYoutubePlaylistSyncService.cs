namespace Ludokino.Api.Services.Interfaces;

public interface IYoutubePlaylistSyncService
{
    Task<int> SynchronizeAsync(CancellationToken cancellationToken = default);
}