namespace Web.Ui.Services;

public interface IClientLoggingService
{
    Task LogErrorAsync(string message, Exception? exception = null, string? source = null);
    Task LogWarningAsync(string message, string? source = null);
}

public class ClientLoggingService(IServiceScopeFactory serviceScopeFactory,  ILogger<ClientLoggingService> consoleLogger)
    : IClientLoggingService
{
    public Task LogErrorAsync(string message, Exception? exception = null, string? source = null)
        => SendAsync("Error", message, exception, source);

    public Task LogWarningAsync(string message, string? source = null)
        => SendAsync("Warning", message, null, source);

    private async Task SendAsync(string level, string message, Exception? exception, string? source)
    {
        //todo make list and send list instead of one by one
        consoleLogger.LogError(exception, "[{Source}] {Message}", source, message);

        try
        {
            var fakeScope = serviceScopeFactory.CreateScope();
            var client = fakeScope.ServiceProvider.GetRequiredService<IApiClient>();
            
            var entry = new ClientLogEntry(
                exception?.ToString(),
                level,
                message,
                source,
                DateTime.UtcNow
            );

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await client.LogClientEntriesAsync([entry], cts.Token);
        }
        catch (Exception ex)
        {
            consoleLogger.LogError(ex, ex.Message);
        }
    }
}