using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.DTOs.Categories;

public class CreateCategoryRequest
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;
}
