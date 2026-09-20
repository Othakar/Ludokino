namespace Ludokino.Api.DTOs.Emissions;

public class UpdateEmissionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? PlaylistUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool? IsFeatured { get; set; }
}
