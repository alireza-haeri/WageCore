namespace Infrastructure.Services;

public class JwtTokenService(IOptions<ApplicationSettings> options) : IJwtTokenService
{
    private readonly JwtTokenSettings _jwtTokenSettings = options.Value.JwtToken;

    public JwtTokenResponse GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenSettings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName)
        ];
        if (user.PhoneNumber != null)
            claims.Add(new(ClaimTypes.MobilePhone, user.PhoneNumber));
        if (user.Email != null)
            claims.Add(new(ClaimTypes.Email, user.Email));

        var token = new JwtSecurityToken(
            audience: _jwtTokenSettings.Audience,
            issuer: _jwtTokenSettings.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtTokenSettings.ExpiresInMinutes),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtTokenResponse(tokenString, _jwtTokenSettings.ExpiresInMinutes);
    }
}