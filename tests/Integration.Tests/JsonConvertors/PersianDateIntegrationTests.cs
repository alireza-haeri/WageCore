namespace Integration.Tests.JsonConvertors;

public class PersianDateIntegrationTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task PostDate_WithValidPersianDate_ShouldReturnOk()
    {
        var request = new { Date = "1403/05/25" };

        var response = await _client.PostAsJsonAsync("/api/test/datetime/date", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.ShouldBeSuccess<DateResponse>();
        result.Date.Should().Be("1403/05/25");
    }

    [Fact]
    public async Task PostDate_WithInvalidPersianDate_ShouldReturnBadRequest()
    {
        var request = new { Date = "invalid" };

        var response = await _client.PostAsJsonAsync("/api/test/datetime/date", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record DateResponse(string Date);
}