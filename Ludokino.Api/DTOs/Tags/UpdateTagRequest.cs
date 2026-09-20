using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.DTOs.Tags;

public class UpdateTagRequest
{
    [MinLength(2)]
    public string? Name { get; set; }
}
