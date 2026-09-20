namespace Ludokino.Api.DTOs.Articles;

public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = "Draft";
    public List<int> CategoryIds { get; set; } = new();
    public List<int> TagIds { get; set; } = new();
    public List<int> EmissionIds { get; set; } = new();
    public List<int> AuthorIds { get; set; } = new();
}
