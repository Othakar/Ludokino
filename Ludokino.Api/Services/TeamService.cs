using Ludokino.Api.Data;
using Ludokino.Api.DTOs.Team;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Services;

public class TeamService : ITeamService
{
    private readonly AppDbContext _context;

    public TeamService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeamMemberDto>> GetVisibleMembersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.IsVisibleOnTeamPage)
            .OrderBy(u => u.Name)
            .Select(u => new TeamMemberDto
            {
                Id = u.Id,
                FirstName = u.Name,
                LastName = string.Empty,
                Function = u.Pseudo,
                Bio = u.Bio,
                AvatarUrl = u.AvatarUrl,
                IsVisibleOnTeamPage = u.IsVisibleOnTeamPage,
                RoleName = u.Role.Name
            })
            .ToListAsync();
    }

    public async Task<List<TeamMemberDto>> GetAllMembersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderBy(u => u.Name)
            .Select(u => new TeamMemberDto
            {
                Id = u.Id,
                FirstName = u.Name,
                LastName = string.Empty,
                Function = u.Pseudo,
                Bio = u.Bio,
                AvatarUrl = u.AvatarUrl,
                IsVisibleOnTeamPage = u.IsVisibleOnTeamPage,
                RoleName = u.Role.Name
            })
            .ToListAsync();
    }
}
