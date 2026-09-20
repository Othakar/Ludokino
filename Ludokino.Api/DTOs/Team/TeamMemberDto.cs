namespace Ludokino.Api.DTOs.Team;

public class TeamMemberDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Function { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsVisibleOnTeamPage { get; set; }
    public string? RoleName { get; set; }
}
