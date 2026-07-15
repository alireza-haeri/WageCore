namespace Shared.Kernel.Extensions;

public static class ApplicationSettingsExtensions
{
    public static ApplicationSettings GetApplicationSettings(this WebApplicationBuilder builder) =>
        builder.Configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>()
        ?? throw new InvalidOperationException(nameof(ApplicationSettings));
}