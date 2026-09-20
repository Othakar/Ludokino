namespace Ludokino.Api.Services.Interfaces;

public interface IRepositoryService
{
    Task<int> GetArticlesCountAsync();
    Task<int> GetDraftsCountAsync();
    Task<int> GetVisibleUsersCountAsync();
}
