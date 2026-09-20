using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.DTOs.Tags;

public class CreateTagRequest
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;
}
