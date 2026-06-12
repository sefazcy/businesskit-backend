using BusinessKit.Application.Auth;
using BusinessKit.Application.Auth.Dtos;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var tokenResult = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.FullName, roles);

        return new LoginResponseDto
        {
            Token = tokenResult.Token,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
            ExpiresAt = tokenResult.ExpiresAt
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
            return null;

        return new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }
}
