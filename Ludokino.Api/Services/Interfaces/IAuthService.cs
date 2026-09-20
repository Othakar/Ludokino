using Ludokino.Api.DTOs.Auth;

namespace Ludokino.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}
