namespace Integration.Tests.Api;

public class EmployeeSalaryProfileControllerTests(ApiFixture fixture) : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = fixture.CreateClient();

    private const string EmployeeSalaryProfilesUrl = "/api/v1/employee-salary-profiles";

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetEmployeeSalaryProfiles_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync(
            $"{EmployeeSalaryProfilesUrl}?Pagination.PageNumber=1&Pagination.PageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateEmployeeSalaryProfile_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(EmployeeSalaryProfilesUrl, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateEmployeeSalaryProfile_WithoutToken_ShouldReturnUnauthorized()
    {
        var salaryProfileId = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync($"{EmployeeSalaryProfilesUrl}/{salaryProfileId}", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteEmployeeSalaryProfile_WithoutToken_ShouldReturnUnauthorized()
    {
        var salaryProfileId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"{EmployeeSalaryProfilesUrl}/{salaryProfileId}?employeeId={employeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
