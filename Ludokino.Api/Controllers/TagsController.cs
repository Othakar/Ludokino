using Ludokino.Api.DTOs.Tags;
using Ludokino.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ludokino.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _tagService.GetAllAsync());

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var tag = await _tagService.GetBySlugAsync(slug);
        return tag is null ? NotFound() : Ok(tag);
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        try
        {
            var tag = await _tagService.CreateAsync(request);
            return CreatedAtAction(nameof(GetBySlug), new { slug = tag.Slug }, tag);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTagRequest request)
    {
        try
        {
            var tag = await _tagService.UpdateAsync(id, request);
            return tag is null ? NotFound() : Ok(tag);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _tagService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
