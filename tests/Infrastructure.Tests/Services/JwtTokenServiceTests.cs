using SamarPlanner.Shared.Tests.Assertions;
using SamarPlanner.Shared.Tests.Helpers;

namespace SamarPlanner.Identity.Infrastructure.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly JwtTokenSettings _jwtSettings;
    private readonly UserBuilder  _userBuilder;

    public JwtTokenServiceTests()
    {
        _jwtSettings = TestApplicationSettings.DefaultJwtSettings;
        var options = TestApplicationSettings.Create(_jwtSettings);
        _service = new JwtTokenService(options);
        _userBuilder = new UserBuilder();
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnNonEmptyToken()
    {
        var user = _userBuilder.CreateResult().ShouldBeSuccess();

        var result = _service.GenerateToken(user);

        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ShouldReturnCorrectExpiresInMinutes()
    {
        var user = _userBuilder.CreateResult().ShouldBeSuccess();

        var result = _service.GenerateToken(user);

        result.ExpiresInMinutes.Should().Be(_jwtSettings.ExpiresInMinutes);
    }

    [Fact]
    public void GenerateToken_ShouldContainUserIdClaim()
    {
        var user = _userBuilder.CreateResult().ShouldBeSuccess();

        var result = _service.GenerateToken(user);

        var claims = JwtTokenHelper.GetTokenClaims(result.Token);
        claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier &&
            c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldContainPhoneNumberClaim()
    {
        var validPhoneNumber = "09123456789";
        var user = _userBuilder.WithPhoneNumber(validPhoneNumber).CreateResult().ShouldBeSuccess();
        
        var result = _service.GenerateToken(user);

        var claims = JwtTokenHelper.GetTokenClaims(result.Token);
        claims.Should().Contain(c =>
            c.Type == ClaimTypes.MobilePhone &&
            c.Value == validPhoneNumber);
    }

    [Fact]
    public void GenerateToken_ShouldHaveCorrectIssuer()
    {
        var user = _userBuilder.CreateResult().ShouldBeSuccess()!;

        var result = _service.GenerateToken(user);

        var token = JwtTokenHelper.GetToken(result.Token)!;
        token.Issuer.Should().Be(_jwtSettings.Issuer);
    }

    [Fact]
    public void GenerateToken_ShouldHaveCorrectAudience()
    {
        var user = _userBuilder.CreateResult().ShouldBeSuccess();

        var result = _service.GenerateToken(user);

        var token = JwtTokenHelper.GetToken(result.Token)!;
        token.Audiences.Should().Contain(_jwtSettings.Audience);
    }

    [Fact]
    public void GenerateToken_TokenShouldBeValidatable()
    {
        var user = _userBuilder.CreateResult().ShouldBeSuccess()!;

        var result = _service.GenerateToken(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true
        };

        var validateAction = () => tokenHandler.ValidateToken(
            result.Token, validationParams, out _);

        validateAction.Should().NotThrow();
    }
  
}