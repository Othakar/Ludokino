using Ludokino.Api.DTOs.Articles;
using Ludokino.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ludokino.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var articles = await _articleService.GetPublishedAsync(page, pageSize);
        return Ok(articles);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var articles = await _articleService.GetFeaturedAsync();
        return Ok(articles);
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var article = await _articleService.GetByIdAsync(id, includeDrafts: true);
        if (article is null)
        {
            return NotFound();
        }

        return Ok(article);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var article = await _articleService.GetBySlugAsync(slug);
        if (article is null)
        {
            return NotFound();
        }

        return Ok(article);
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpGet("drafts")]
    public async Task<IActionResult> GetDrafts()
    {
        var drafts = await _articleService.GetDraftsAsync();
        return Ok(drafts);
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest request)
    {
        try
        {
            var article = await _articleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetBySlug), new { slug = article.Slug }, article);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateArticleRequest request)
    {
        var article = await _articleService.UpdateAsync(id, request);
        return article is null ? NotFound() : Ok(article);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _articleService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
