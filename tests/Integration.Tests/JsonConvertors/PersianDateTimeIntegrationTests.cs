namespace Integration.Tests.JsonConvertors;

public class PersianDateTimeIntegrationTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task PostDateTime_WithValidPersianDateTime_ShouldReturnOk()
    {
        var request = new { DateTime = "1403/05/25 14:30" };

        var response = await _client.PostAsJsonAsync("/api/test/datetime/datetime", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.ShouldBeSuccess<DateTimeResponse>();
        result.DateTime.Should().Be("1403/05/25 14:30");
    }

    [Fact]
    public async Task PostDateTime_WithInvalidPersianDateTime_ShouldReturnBadRequest()
    {
        var request = new { DateTime = "invalid" };

        var response = await _client.PostAsJsonAsync("/api/test/datetime/datetime", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record DateTimeResponse(string DateTime);
}