// src/SamarPlanner.Api/Controllers/ClientLogsController.cs
using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/v1/client-logs")]
[AllowAnonymous]
public class ClientLogsController(ILogger<ClientLogsController> logger) : ControllerBase
{
    public record ClientLogEntry(
        string Level,
        string Message,
        string? Exception,
        string? Source,
        DateTime Timestamp);

    [HttpPost]
    [SwaggerOperation(OperationId = "LogClientEntries")]
    public IActionResult Post([FromBody] List<ClientLogEntry> entries)
    {
        //todo add rate limit
        foreach (var entry in entries)
        {
            var level = entry.Level switch
            {
                "Error" => LogLevel.Error,
                "Warning" => LogLevel.Warning,
                "Critical" => LogLevel.Critical,
                _ => LogLevel.Information
            };

            logger.Log(level,
                "[CLIENT] {Source} | {Message} {Exception}",
                entry.Source, entry.Message, entry.Exception);
        }

        return Ok();
    }
}