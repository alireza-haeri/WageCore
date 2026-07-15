namespace SamarPlanner.Shared.Tests.Helpers;

public static class TestApplicationSettings
{
    public static readonly JwtTokenSettings DefaultJwtSettings = new()
    {
        SigningKey = "this-is-a-test-signing-key-that-is-long-enough-32!",
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpiresInMinutes = 60
    };

    public static IOptions<ApplicationSettings> Create(
        JwtTokenSettings? jwtSettings = null)
    {
        var settings = new ApplicationSettings
        {
            JwtToken = jwtSettings ?? DefaultJwtSettings,
            Databases = new DatabaseSettings
            {
                IdentityConnectionString = "fake",
                GoalConnectionString = "fake",
                TaskConnectionString = "fake",
                ReportConnectionString = "fake",
                NoteConnectionString = "fake"
            },
            CorsPolicy = new CorsPolicySettings
            {
                Origins = ["http://localhost"]
            },
            NoteFiles = new NoteFilesSettings
            {
                FilesPath = "/tmp/test"
            }
        };

        return Options.Create(settings);
    }
}