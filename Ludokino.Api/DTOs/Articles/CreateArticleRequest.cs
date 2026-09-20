using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.DTOs.Articles;

public class CreateArticleRequest
{
    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Excerpt { get; set; } = string.Empty;
    [Required]
    public string Content { get; set; } = string.Empty;
    [Url]
    public string? CoverImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    [Url]
    public string? VideoUrl { get; set; }
    public bool IsFeatured { get; set; }
    [Required]
    public string Status { get; set; } = "Draft";
    public List<int> CategoryIds { get; set; } = new();
    public List<int> TagIds { get; set; } = new();
    public List<int> EmissionIds { get; set; } = new();
    public List<int> AuthorIds { get; set; } = new();
}
