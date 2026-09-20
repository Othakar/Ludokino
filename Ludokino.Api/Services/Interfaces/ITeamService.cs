using Ludokino.Api.DTOs.Team;

namespace Ludokino.Api.Services.Interfaces;

public interface ITeamService
{
    Task<List<TeamMemberDto>> GetVisibleMembersAsync();
    Task<List<TeamMemberDto>> GetAllMembersAsync();
}
