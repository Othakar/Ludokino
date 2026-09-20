using Ludokino.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ludokino.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Redacteur")]
public class DashboardController : ControllerBase
{
    private readonly IRepositoryService _repositoryService;

    public DashboardController(IRepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = new
        {
            publishedArticles = await _repositoryService.GetArticlesCountAsync(),
            drafts = await _repositoryService.GetDraftsCountAsync(),
            teamMembers = await _repositoryService.GetVisibleUsersCountAsync()
        };

        return Ok(stats);
    }
}
