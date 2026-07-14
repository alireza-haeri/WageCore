namespace Shared.Kernel.Common;

public sealed class ApplicationSettings
{
    public required string ApplicationName { get; init; }
    public required string ApplicationVersion { get; init; }
    public required JwtTokenSettings JwtToken { get; init; }
    public required DatabaseSettings Databases { get; init; }
    public required CorsPolicySettings CorsPolicy { get; init; }
}

public sealed class DatabaseSettings
{
    public required string ConnectionStirng { get; init; }
}

public sealed class JwtTokenSettings
{
    public required string SigningKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int ExpiresInMinutes { get; init; } = 10080;
}

public sealed class CorsPolicySettings
{
    public required string[] Origins { get; init; }
}