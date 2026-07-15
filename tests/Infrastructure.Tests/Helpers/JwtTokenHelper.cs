namespace SamarPlanner.Identity.Infrastructure.Tests.Helpers;

public static class JwtTokenHelper
{
    public static List<Claim> GetTokenClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.ToList();
    }

    public static JwtSecurityToken? GetToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken;
    }
}