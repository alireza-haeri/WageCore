namespace Core.Abstractions.Services;

public interface IJwtTokenService
{
    JwtTokenResponse GenerateToken(User user, IReadOnlyCollection<string>? roles = null);
}