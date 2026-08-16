namespace Integration.Tests.Api;

public class UsersControllerTests(ApiFixture fixture) : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = fixture.CreateClient();

    private const string ValidPhoneNumber = "09123456789";
    private const string ValidEmail = "ali@gmail.com";
    private const string ValidPassword = "123456";
    private const string ValidFullName = "علی رضایی";

    private const string RegisterUrl = "/api/v1/users/register";
    private const string LoginUrl = "/api/v1/users/login";

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOkWithToken()
    {
        var request = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        var result = await _client.PostAsJsonAsync(RegisterUrl, request);
        var raw = await result.Content.ReadAsStringAsync();
        
        var response = await result.ShouldBeSuccess<RegisterUserCommandResponse>();
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpireInMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithValidData_ShouldReturnOkWithToken()
    {
        var registerRequest = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);
        var loginRequest = new LoginUserCommand(ValidPhoneNumber, ValidEmail, ValidPassword);

        await _client.PostAsJsonAsync(RegisterUrl, registerRequest);
        
        var result = await _client.PostAsJsonAsync(LoginUrl, loginRequest);
        
        var response = await result.ShouldBeSuccess<LoginUserCommandResponse>();
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpireInMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Register_WithInvalidPhoneNumber_ShouldReturnBadRequest()
    {
        var request = new RegisterUserCommand("123", ValidEmail, ValidFullName, ValidPassword);

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        await response.ShouldBeFailure<RegisterUserCommandResponse>(BadResultType.Validation);
    }

    [Fact]
    public async Task Register_WithShortPassword_ShouldReturnBadRequest()
    {
        var request = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, "123");

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        await response.ShouldBeFailure<RegisterUserCommandResponse>(BadResultType.Validation);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterUserCommand(ValidPhoneNumber, "invalid-email", ValidFullName, ValidPassword);

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        await response.ShouldBeFailure<RegisterUserCommandResponse>(BadResultType.Validation);
    }

    [Fact]
    public async Task Register_WithEmptyFullName_ShouldReturnBadRequest()
    {
        var request = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, "", ValidPassword);

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        await response.ShouldBeFailure<RegisterUserCommandResponse>(BadResultType.Validation);
    }

    [Fact]
    public async Task Register_DuplicateUser_ShouldReturnConflict()
    {
        var request = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        await _client.PostAsJsonAsync(RegisterUrl, request);

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        var failure = await response.ShouldBeFailure<RegisterUserCommandResponse>();
        failure.Should().ContainKey("PhoneNumber");
        failure.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Register_WithOnlyPhoneNumber_ShouldSucceed()
    {
        var request = new RegisterUserCommand(ValidPhoneNumber, null, ValidFullName, ValidPassword);

        var result = await _client.PostAsJsonAsync(RegisterUrl, request);
        var response = await result.ShouldBeSuccess<RegisterUserCommandResponse>();
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithOnlyEmail_ShouldSucceed()
    {
        var request = new RegisterUserCommand(null, ValidEmail, ValidFullName, ValidPassword);

        var result = await _client.PostAsJsonAsync(RegisterUrl, request);
        var response = await result.ShouldBeSuccess<RegisterUserCommandResponse>();
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithBothPhoneAndEmailNull_ShouldReturnBadRequest()
    {
        var request = new RegisterUserCommand(null, null, ValidFullName, ValidPassword);

        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        await response.ShouldBeFailure<RegisterUserCommandResponse>(BadResultType.Validation);
    }
}