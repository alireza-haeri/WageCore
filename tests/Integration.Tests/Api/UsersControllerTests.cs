using SamarPlanner.Shared.Tests.Assertions;

namespace SamarPlanner.Identity.Integration.Tests.Api;

public class IdentityControllerTests(IdentityApiFixture fixture) : IClassFixture<IdentityApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = fixture.CreateClient();

    private const string ValidPhoneNumber = "09123456789";
    private const string ValidPassword = "Pass123456";

    private const string RegisterOrLoginUrl = "/api/v1/identity/authentication";

    public async System.Threading.Tasks.Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public System.Threading.Tasks.Task DisposeAsync() => System.Threading.Tasks.Task.CompletedTask;

    [Fact]
    public async System.Threading.Tasks.Task Register_WithValidData_ShouldReturnOkWithToken()
    {
        var request = new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword);

        var result = await _client.PostAsJsonAsync(RegisterOrLoginUrl, request);
        var raw = await result.Content.ReadAsStringAsync();
        
        var response = await result.ShouldBeSuccess<RegisterOrLoginCommandResponse>();
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpireInMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Register_WithInvalidData_ShouldReturnBadRequest()
    {
        var request = new RegisterOrLoginCommand("1", "123");

        var response = await _client.PostAsJsonAsync(RegisterOrLoginUrl, request);
        await response.ShouldBeFailure<RegisterOrLoginCommandResponse>(BadResultType.Validation);
    }

    [Fact]
    public async System.Threading.Tasks.Task Login_WithWrongPassword_ShouldNotReturnToken()
    {
        var request = new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword);
        var request2 = new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword + "something");

        await _client.PostAsJsonAsync(RegisterOrLoginUrl, request);

        var response = await _client.PostAsJsonAsync(RegisterOrLoginUrl, request2);
        await response.ShouldBeFailure<RegisterOrLoginCommandResponse>(BadResultType.NotFound);
    }
}