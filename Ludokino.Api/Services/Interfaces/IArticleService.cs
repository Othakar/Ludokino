using Ludokino.Api.DTOs.Articles;

namespace Ludokino.Api.Services.Interfaces;

public interface IArticleService
{
    Task<List<ArticleSummaryDto>> GetPublishedAsync(int page = 1, int pageSize = 12);
    Task<List<ArticleSummaryDto>> GetFeaturedAsync();
    Task<ArticleDto?> GetBySlugAsync(string slug, bool includeDrafts = false);
    Task<List<ArticleSummaryDto>> GetDraftsAsync();
    Task<ArticleDto> CreateAsync(CreateArticleRequest request);
    Task<ArticleDto?> UpdateAsync(int id, UpdateArticleRequest request);
    Task<bool> DeleteAsync(int id);
}
