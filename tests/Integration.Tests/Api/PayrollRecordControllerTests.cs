namespace Integration.Tests.Api;

public class PayrollRecordControllerTests(ApiFixture fixture) : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = fixture.CreateClient();

    private const string PayrollRecordsUrl = "/api/v1/payroll-records";

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SavePayrollRecord_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.PutAsJsonAsync(PayrollRecordsUrl, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePayrollRecord_WithoutToken_ShouldReturnUnauthorized()
    {
        var payrollRecordId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"{PayrollRecordsUrl}/{payrollRecordId}?employeeId={employeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MarkPayrollRecordAsPaid_WithoutToken_ShouldReturnUnauthorized()
    {
        var payrollRecordId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"{PayrollRecordsUrl}/{payrollRecordId}/mark-as-paid?employeeId={employeeId}",
            new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
