namespace Integration.Tests.Controllers;

[ApiController]
[Route("api/test/datetime")]
public class TestDateTimeController : ControllerBase
{
    [HttpPost("date")]
    public Result<DateResponse> PostDate([FromBody] DateRequest request)
    {
        return Result<DateResponse>.Success(new DateResponse(request.Date.ToDisplay()));
    }

    [HttpPost("time")]
    public Result<TimeResponse> PostTime([FromBody] TimeRequest request)
    {
        return Result<TimeResponse>.Success(new TimeResponse(request.Time.ToDisplay()));
    }

    [HttpPost("datetime")]
    public Result<DateTimeResponse> PostDateTime([FromBody] DateTimeRequest request)
    {
        return Result<DateTimeResponse>.Success(new DateTimeResponse(request.DateTime.ToDisplay()));
    }

    public record DateRequest(PersianDate Date);
    public record TimeRequest(PersianTime Time);
    public record DateTimeRequest(PersianDateTime DateTime);

    public record DateResponse(string Date);
    public record TimeResponse(string Time);
    public record DateTimeResponse(string DateTime);
}