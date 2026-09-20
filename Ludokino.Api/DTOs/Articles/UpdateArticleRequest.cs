namespace Ludokino.Api.DTOs.Articles;

public class UpdateArticleRequest
{
    public string? Title { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool? IsFeatured { get; set; }
    public string? Status { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? TagIds { get; set; }
    public List<int>? EmissionIds { get; set; }
    public List<int>? AuthorIds { get; set; }
}
