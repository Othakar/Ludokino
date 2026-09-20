using Ludokino.Api.DTOs.Team;

namespace Ludokino.Api.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}
