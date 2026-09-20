using Ludokino.Api.DTOs.Auth;

namespace Ludokino.Api.DTOs.Articles;

public class ArticleSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? PublishedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<UserDto> Authors { get; set; } = new();
}
