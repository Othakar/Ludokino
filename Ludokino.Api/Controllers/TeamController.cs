using Ludokino.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ludokino.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVisibleMembers()
    {
        var members = await _teamService.GetVisibleMembersAsync();
        return Ok(members);
    }

    [Authorize(Roles = "Admin,Redacteur")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllMembers()
    {
        var members = await _teamService.GetAllMembersAsync();
        return Ok(members);
    }
}
