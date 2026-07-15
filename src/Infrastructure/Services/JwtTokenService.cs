using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SamarPlanner.Identity.Core.Abstractions;
using SamarPlanner.Identity.Core.Contracts;
using SamarPlanner.Identity.Core.Entities;
using SamarPlanner.Shared.Kernel;

namespace SamarPlanner.Identity.Infrastructure.Services;

public class JwtTokenService(IOptions<ApplicationSettings> options) : IJwtTokenService
{
    private readonly JwtTokenSettings _jwtTokenSettings = options.Value.JwtToken;

    public JwtTokenResponse GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenSettings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            audience: _jwtTokenSettings.Audience,
            issuer: _jwtTokenSettings.Issuer,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
            ],
            expires: DateTime.UtcNow.AddMinutes(_jwtTokenSettings.ExpiresInMinutes),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtTokenResponse(tokenString, _jwtTokenSettings.ExpiresInMinutes);
    }
}