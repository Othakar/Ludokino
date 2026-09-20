namespace Ludokino.Api.DTOs.Emissions;

public class EmissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string YoutubeUrl { get; set; } = string.Empty;
    public string? PlaylistUrl { get; set; }
    public string? LatestVideoId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? ImageUrl { get; set; }
}
