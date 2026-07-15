
namespace Shared.Tests.Helpers;

public static class TestApplicationSettings
{
    public static readonly JwtTokenSettings DefaultJwtSettings = new()
    {
        SigningKey = "this-is-a-test-signing-key-that-is-long-enough-32!",
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpiresInMinutes = 60
    };

    public static IOptions<ApplicationSettings> Create()
    {
        var settings = new ApplicationSettings
        {
            ApplicationName = "test-application",
            ApplicationVersion = "test-application-version",
            JwtToken = DefaultJwtSettings,
            Databases = new DatabaseSettings
            {
                ConnectionString = "fake"
            },
            CorsPolicy = new CorsPolicySettings
            {
                Origins = ["http://localhost"]
            }
        };

        return Options.Create(settings);
    }
}