namespace Ludokino.Api.Models;

public class Emission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string YoutubeUrl { get; set; } = string.Empty;
    public string? PlaylistUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsFeatured { get; set; }

    public ICollection<ArticleEmission> ArticleEmissions { get; set; } = new List<ArticleEmission>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
