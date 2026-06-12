namespace BusinessKit.Application.Auth;

public record JwtTokenResult(string Token, DateTime ExpiresAt);

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(int userId, string email, string fullName, IEnumerable<string> roles);
}
