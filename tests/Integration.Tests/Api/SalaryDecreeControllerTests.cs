namespace Integration.Tests.Api;

public class SalaryDecreeControllerTests(ApiFixture fixture) : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = fixture.CreateClient();

    private const string SalaryDecreesUrl = "/api/v1/salary-decrees";

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSalaryDecrees_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync(
            $"{SalaryDecreesUrl}?Pagination.PageNumber=1&Pagination.PageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSalaryDecree_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(SalaryDecreesUrl, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSalaryDecree_WithoutToken_ShouldReturnUnauthorized()
    {
        var salaryProfileId = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync($"{SalaryDecreesUrl}/{salaryProfileId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteSalaryDecree_WithoutToken_ShouldReturnUnauthorized()
    {
        var salaryProfileId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"{SalaryDecreesUrl}/{salaryProfileId}?employeeId={employeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
