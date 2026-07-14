namespace Shared.Web.Extensions;

public static class SerilogConfigurationExtension
{
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(builder.Environment.ContentRootPath, "Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day
            )
        );
        return builder;
    }
}