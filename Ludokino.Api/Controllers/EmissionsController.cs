using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ludokino.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmissionsController : ControllerBase
{
    private readonly IEmissionService _emissionService;
    private readonly IYoutubePlaylistSyncService _youtubeSyncService;

    public EmissionsController(IEmissionService emissionService, IYoutubePlaylistSyncService youtubeSyncService)
    {
        _emissionService = emissionService;
        _youtubeSyncService = youtubeSyncService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _emissionService.GetAllAsync());

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured() => Ok(await _emissionService.GetFeaturedAsync());

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var emission = await _emissionService.GetBySlugAsync(slug);
        return emission is null ? NotFound() : Ok(emission);
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmissionRequest request)
    {
        try
        {
            var emission = await _emissionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetBySlug), new { slug = emission.Slug }, emission);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmissionRequest request)
    {
        try
        {
            var emission = await _emissionService.UpdateAsync(id, request);
            return emission is null ? NotFound() : Ok(emission);
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
        return await _emissionService.DeleteAsync(id) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("sync-youtube")]
    public async Task<IActionResult> SynchronizeYoutube(CancellationToken cancellationToken)
    {
        var synchronizedCount = await _youtubeSyncService.SynchronizeAsync(cancellationToken);
        return Ok(new { synchronizedCount });
    }
}
