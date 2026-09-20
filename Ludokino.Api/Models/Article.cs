namespace Ludokino.Api.Models;

public enum ArticleStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }
    public bool IsFeatured { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime? PublishedAt { get; set; }

    public ICollection<ArticleAuthor> ArticleAuthors { get; set; } = new List<ArticleAuthor>();
    public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    public ICollection<ArticleEmission> ArticleEmissions { get; set; } = new List<ArticleEmission>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
