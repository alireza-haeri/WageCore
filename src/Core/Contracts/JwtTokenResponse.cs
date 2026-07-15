namespace Core.Contracts;

public record JwtTokenResponse(string Token, int ExpiresInMinutes);