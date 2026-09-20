using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.DTOs.Categories;

public class UpdateCategoryRequest
{
    [MinLength(2)]
    public string? Name { get; set; }
}
