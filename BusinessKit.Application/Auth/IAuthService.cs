using BusinessKit.Application.Auth.Dtos;

namespace BusinessKit.Application.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<CurrentUserDto?> GetCurrentUserAsync(int userId);
}
