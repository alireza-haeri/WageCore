// src/SamarPlanner.Web.Shared/Components/LoggingErrorBoundary.cs

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Web.Ui.Services;

public class LoggingErrorBoundary : ErrorBoundary
{
    [Inject] private IClientLoggingService LoggingService { get; set; } = null!;

    protected override async Task OnErrorAsync(Exception exception)
    {
        await LoggingService.LogErrorAsync(
            message: "Component render error",
            exception: exception,
            source: "ErrorBoundary");

        await base.OnErrorAsync(exception);
    }
}