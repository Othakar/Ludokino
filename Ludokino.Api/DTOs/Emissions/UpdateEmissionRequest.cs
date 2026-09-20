using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.DTOs.Emissions;

public class UpdateEmissionRequest
{
    [MinLength(2)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    [Url]
    public string? YoutubeUrl { get; set; }
    [Url]
    public string? PlaylistUrl { get; set; }
    [Url]
    public string? ThumbnailUrl { get; set; }
    public bool? IsFeatured { get; set; }
}
