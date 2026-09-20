using System.Text.RegularExpressions;
using Ludokino.Api.Data;
using Ludokino.Api.DTOs.Articles;
using Ludokino.Api.DTOs.Auth;
using Ludokino.Api.DTOs.Categories;
using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.DTOs.Tags;
using Ludokino.Api.Models;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Services;

public class ArticleService : IArticleService
{
    private readonly AppDbContext _context;

    public ArticleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArticleSummaryDto>> GetPublishedAsync(int page = 1, int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 12;

        var articles = await _context.Articles
            .AsNoTracking()
            .Where(a => a.Status == ArticleStatus.Published)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.ArticleAuthors)
                .ThenInclude(aa => aa.User)
            .Include(a => a.ArticleCategories)
                .ThenInclude(ac => ac.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .ToListAsync();

        return articles.Select(MapSummary).ToList();
    }

    public async Task<List<ArticleSummaryDto>> GetFeaturedAsync()
    {
        var articles = await _context.Articles
            .AsNoTracking()
            .Where(a => a.Status == ArticleStatus.Published && a.IsFeatured)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Take(5)
            .Include(a => a.ArticleAuthors)
                .ThenInclude(aa => aa.User)
            .Include(a => a.ArticleCategories)
                .ThenInclude(ac => ac.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .ToListAsync();

        return articles.Select(MapSummary).ToList();
    }

    public async Task<ArticleDto?> GetByIdAsync(int id, bool includeDrafts = false)
    {
        var query = _context.Articles
            .AsNoTracking()
            .Include(a => a.ArticleAuthors)
                .ThenInclude(aa => aa.User)
                .ThenInclude(u => u.Role)
            .Include(a => a.ArticleCategories)
                .ThenInclude(ac => ac.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.ArticleEmissions)
                .ThenInclude(ae => ae.Emission)
            .AsQueryable();

        if (!includeDrafts)
        {
            query = query.Where(a => a.Status == ArticleStatus.Published);
        }

        var article = await query.FirstOrDefaultAsync(a => a.Id == id);
        return article is null ? null : MapDetail(article);
    }

    public async Task<ArticleDto?> GetBySlugAsync(string slug, bool includeDrafts = false)
    {
        var query = _context.Articles
            .AsNoTracking()
            .Include(a => a.ArticleAuthors)
                .ThenInclude(aa => aa.User)
                .ThenInclude(u => u.Role)
            .Include(a => a.ArticleCategories)
                .ThenInclude(ac => ac.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.ArticleEmissions)
                .ThenInclude(ae => ae.Emission)
            .AsQueryable();

        if (!includeDrafts)
        {
            query = query.Where(a => a.Status == ArticleStatus.Published);
        }

        var article = await query.FirstOrDefaultAsync(a => a.Slug == slug.Trim());
        return article is null ? null : MapDetail(article);
    }

    public async Task<List<ArticleSummaryDto>> GetDraftsAsync()
    {
        var articles = await _context.Articles
            .AsNoTracking()
            .Where(a => a.Status == ArticleStatus.Draft)
            .OrderByDescending(a => a.UpdatedAt)
            .Include(a => a.ArticleAuthors)
                .ThenInclude(aa => aa.User)
            .Include(a => a.ArticleCategories)
                .ThenInclude(ac => ac.Category)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .ToListAsync();

        return articles.Select(MapSummary).ToList();
    }

    public async Task<ArticleDto> CreateAsync(CreateArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Le titre est obligatoire.");
        }

        var status = ParseStatus(request.Status);
        var slug = GenerateSlug(request.Title);

        var article = new Article
        {
            Title = request.Title.Trim(),
            Slug = await EnsureUniqueSlugAsync(slug),
            Excerpt = request.Excerpt ?? string.Empty,
            Content = request.Content ?? string.Empty,
            CoverImageUrl = request.CoverImageUrl,
            ImageUrls = request.ImageUrls ?? new List<string>(),
            VideoUrl = request.VideoUrl,
            IsFeatured = request.IsFeatured,
            Status = status,
            PublishedAt = status == ArticleStatus.Published ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        await AttachRelationsAsync(article.Id, request.CategoryIds, request.TagIds, request.EmissionIds, request.AuthorIds);

        return await GetBySlugAsync(article.Slug, includeDrafts: true) ?? MapDetail(article);
    }

    public async Task<ArticleDto?> UpdateAsync(int id, UpdateArticleRequest request)
    {
        var article = await _context.Articles
            .Include(a => a.ArticleAuthors)
            .Include(a => a.ArticleCategories)
            .Include(a => a.ArticleTags)
            .Include(a => a.ArticleEmissions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            article.Title = request.Title.Trim();
            article.Slug = await EnsureUniqueSlugAsync(GenerateSlug(article.Title), article.Id);
        }

        if (request.Excerpt is not null) article.Excerpt = request.Excerpt;
        if (request.Content is not null) article.Content = request.Content;
        if (request.CoverImageUrl is not null) article.CoverImageUrl = request.CoverImageUrl;
        if (request.ImageUrls is not null) article.ImageUrls = request.ImageUrls.Distinct().ToList();
        if (request.VideoUrl is not null) article.VideoUrl = request.VideoUrl;
        if (request.IsFeatured.HasValue) article.IsFeatured = request.IsFeatured.Value;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            article.Status = ParseStatus(request.Status);
            if (article.Status == ArticleStatus.Published && article.PublishedAt is null)
            {
                article.PublishedAt = DateTime.UtcNow;
            }
        }

        article.UpdatedAt = DateTime.UtcNow;

        if (request.CategoryIds is not null) await ReplaceCategoriesAsync(article.Id, request.CategoryIds);
        if (request.TagIds is not null) await ReplaceTagsAsync(article.Id, request.TagIds);
        if (request.EmissionIds is not null) await ReplaceEmissionsAsync(article.Id, request.EmissionIds);
        if (request.AuthorIds is not null) await ReplaceAuthorsAsync(article.Id, request.AuthorIds);

        await _context.SaveChangesAsync();

        return await GetBySlugAsync(article.Slug, includeDrafts: true);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var article = await _context.Articles
            .Include(a => a.ArticleAuthors)
            .Include(a => a.ArticleCategories)
            .Include(a => a.ArticleTags)
            .Include(a => a.ArticleEmissions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article is null)
        {
            return false;
        }

        _context.ArticleAuthors.RemoveRange(article.ArticleAuthors);
        _context.ArticleCategories.RemoveRange(article.ArticleCategories);
        _context.ArticleTags.RemoveRange(article.ArticleTags);
        _context.ArticleEmissions.RemoveRange(article.ArticleEmissions);
        _context.Articles.Remove(article);

        await _context.SaveChangesAsync();
        return true;
    }

    private static ArticleStatus ParseStatus(string status)
    {
        return status.Trim().ToLowerInvariant() switch
        {
            "published" => ArticleStatus.Published,
            "archived" => ArticleStatus.Archived,
            _ => ArticleStatus.Draft
        };
    }

    private static string GenerateSlug(string input)
    {
        var slug = input.Trim();
        slug = Regex.Replace(slug, "[\\s_]+", "-");
        slug = Regex.Replace(slug, "[^a-zA-Z0-9-]", "-");
        slug = Regex.Replace(slug, "-+", "-");
        slug = slug.Trim('-').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(slug) ? "article" : slug;
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, int? excludedArticleId = null)
    {
        var candidate = slug;
        var index = 1;

        while (await _context.Articles.AnyAsync(a => a.Slug == candidate && (!excludedArticleId.HasValue || a.Id != excludedArticleId.Value)))
        {
            candidate = $"{slug}-{index}";
            index++;
        }

        return candidate;
    }

    private async Task AttachRelationsAsync(int articleId, List<int> categoryIds, List<int> tagIds, List<int> emissionIds, List<int> authorIds)
    {
        if (categoryIds.Count > 0)
        {
            await ReplaceCategoriesAsync(articleId, categoryIds);
        }

        if (tagIds.Count > 0)
        {
            await ReplaceTagsAsync(articleId, tagIds);
        }

        if (emissionIds.Count > 0)
        {
            await ReplaceEmissionsAsync(articleId, emissionIds);
        }

        if (authorIds.Count > 0)
        {
            await ReplaceAuthorsAsync(articleId, authorIds);
        }
    }

    private async Task ReplaceCategoriesAsync(int articleId, IEnumerable<int> categoryIds)
    {
        var ids = categoryIds.Distinct().ToList();
        var available = await _context.Categories
            .Where(c => ids.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        var entities = available.Select(id => new ArticleCategory
        {
            ArticleId = articleId,
            CategoryId = id
        }).ToList();

        var existing = await _context.ArticleCategories.Where(ac => ac.ArticleId == articleId).ToListAsync();
        _context.ArticleCategories.RemoveRange(existing);
        _context.ArticleCategories.AddRange(entities);
    }

    private async Task ReplaceTagsAsync(int articleId, IEnumerable<int> tagIds)
    {
        var ids = tagIds.Distinct().ToList();
        var available = await _context.Tags
            .Where(t => ids.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        var entities = available.Select(id => new ArticleTag
        {
            ArticleId = articleId,
            TagId = id
        }).ToList();

        var existing = await _context.ArticleTags.Where(at => at.ArticleId == articleId).ToListAsync();
        _context.ArticleTags.RemoveRange(existing);
        _context.ArticleTags.AddRange(entities);
    }

    private async Task ReplaceEmissionsAsync(int articleId, IEnumerable<int> emissionIds)
    {
        var ids = emissionIds.Distinct().ToList();
        var available = await _context.Emissions
            .Where(e => ids.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync();

        var entities = available.Select(id => new ArticleEmission
        {
            ArticleId = articleId,
            EmissionId = id
        }).ToList();

        var existing = await _context.ArticleEmissions.Where(ae => ae.ArticleId == articleId).ToListAsync();
        _context.ArticleEmissions.RemoveRange(existing);
        _context.ArticleEmissions.AddRange(entities);
    }

    private async Task ReplaceAuthorsAsync(int articleId, IEnumerable<int> authorIds)
    {
        var ids = authorIds.Distinct().ToList();
        var users = await _context.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();

        var entities = users.Select((id, index) => new ArticleAuthor
        {
            ArticleId = articleId,
            UserId = id,
            IsPrimary = index == 0,
            Role = index == 0 ? "author" : "co-author"
        }).ToList();

        var existing = await _context.ArticleAuthors.Where(aa => aa.ArticleId == articleId).ToListAsync();
        _context.ArticleAuthors.RemoveRange(existing);
        _context.ArticleAuthors.AddRange(entities);
    }

    private static ArticleSummaryDto MapSummary(Article article)
    {
        return new ArticleSummaryDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Excerpt = article.Excerpt,
            CoverImageUrl = article.CoverImageUrl,
            ImageUrls = article.ImageUrls,
            VideoUrl = article.VideoUrl,
            PublishedAt = article.PublishedAt?.ToString("O"),
            Tags = article.ArticleTags.Select(at => at.Tag.Name).ToList(),
            Categories = article.ArticleCategories.Select(ac => ac.Category.Name).ToList(),
            Authors = article.ArticleAuthors
                .Select(aa => new UserDto
                {
                    Id = aa.User.Id,
                    Name = aa.User.Name,
                    Pseudo = aa.User.Pseudo,
                    Email = aa.User.Email,
                    AvatarUrl = aa.User.AvatarUrl,
                    Bio = aa.User.Bio,
                    Role = aa.User.Role?.Name ?? string.Empty,
                    IsVisibleOnTeamPage = aa.User.IsVisibleOnTeamPage
                })
                .ToList()
        };
    }

    private static ArticleDto MapDetail(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Excerpt = article.Excerpt,
            Content = article.Content,
            CoverImageUrl = article.CoverImageUrl,
            ImageUrls = article.ImageUrls,
            VideoUrl = article.VideoUrl,
            IsFeatured = article.IsFeatured,
            Status = article.Status.ToString(),
            PublishedAt = article.PublishedAt,
            Authors = article.ArticleAuthors
                .Select(aa => new UserDto
                {
                    Id = aa.User.Id,
                    Name = aa.User.Name,
                    Pseudo = aa.User.Pseudo,
                    Email = aa.User.Email,
                    AvatarUrl = aa.User.AvatarUrl,
                    Bio = aa.User.Bio,
                    Role = aa.User.Role?.Name ?? string.Empty,
                    IsVisibleOnTeamPage = aa.User.IsVisibleOnTeamPage
                })
                .ToList(),
            Categories = article.ArticleCategories
                .Select(ac => new CategoryDto
                {
                    Id = ac.Category.Id,
                    Name = ac.Category.Name,
                    Slug = ac.Category.Slug
                })
                .ToList(),
            Tags = article.ArticleTags
                .Select(at => new TagDto
                {
                    Id = at.Tag.Id,
                    Name = at.Tag.Name,
                    Slug = at.Tag.Slug
                })
                .ToList(),
            Emissions = article.ArticleEmissions
                .Select(ae => new EmissionDto
                {
                    Id = ae.Emission.Id,
                    Name = ae.Emission.Name,
                    Slug = ae.Emission.Slug,
                    Description = ae.Emission.Description,
                    ImageUrl = ae.Emission.ThumbnailUrl
                })
                .ToList()
        };
    }
}
