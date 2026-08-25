namespace Integration.Tests.JsonConvertors;

public class PersianTimeIntegrationTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task PostTime_WithValidPersianTime_ShouldReturnOk()
    {
        var request = new { Time = "14:30" };

        var response = await _client.PostAsJsonAsync("/api/test/datetime/time", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.ShouldBeSuccess<TimeResponse>();
        result.Time.Should().Be("14:30");
    }

    [Fact]
    public async Task PostTime_WithInvalidPersianTime_ShouldReturnBadRequest()
    {
        var request = new { Time = "invalid" };

        var response = await _client.PostAsJsonAsync("/api/test/datetime/time", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record TimeResponse(string Time);
}