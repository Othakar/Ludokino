using System.ComponentModel.DataAnnotations;
using Ludokino.Api.Validation;

namespace Ludokino.Api.DTOs.Articles;

public class UpdateArticleRequest
{
    [MinLength(3)]
    public string? Title { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    [Url]
    public string? CoverImageUrl { get; set; }
    [HttpUrlList]
    public List<string>? ImageUrls { get; set; }
    [Url]
    public string? VideoUrl { get; set; }
    public bool? IsFeatured { get; set; }
    public string? Status { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? TagIds { get; set; }
    public List<int>? EmissionIds { get; set; }
    public List<int>? AuthorIds { get; set; }
}
