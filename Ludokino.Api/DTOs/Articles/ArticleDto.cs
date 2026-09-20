using Ludokino.Api.DTOs.Categories;
using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.DTOs.Tags;
using Ludokino.Api.DTOs.Team;
using Ludokino.Api.DTOs.Auth;

namespace Ludokino.Api.DTOs.Articles;

public class ArticleDto
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
    public string Status { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public List<UserDto> Authors { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
    public List<EmissionDto> Emissions { get; set; } = new();
}
