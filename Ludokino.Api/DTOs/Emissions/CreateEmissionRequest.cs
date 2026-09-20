using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.DTOs.Emissions;

public class CreateEmissionRequest
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    [Required, Url]
    public string YoutubeUrl { get; set; } = string.Empty;
    public string? PlaylistUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsFeatured { get; set; }
}
