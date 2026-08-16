namespace Integration.Tests.Pipeline;

public class RegisterUserPipelineTests(ApiFixture fixture)
    : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private const string ValidPhoneNumber = "09123456789";
    private const string ValidEmail = "ali@gmail.com";
    private const string ValidPassword = "123456";
    private const string ValidFullName = "علی رضایی";

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterNewUser_WithValidData_ShouldSucceed()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        var result = await mediator.Send(command);

        var response = result.ShouldBeSuccess();
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpireInMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Register_DuplicateUser_ShouldFail()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword));

        var result = await mediator.Send(new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword));

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("PhoneNumber");
        failure.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Register_WithInvalidPhoneNumber_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand("123", ValidEmail, ValidFullName, ValidPassword);

        var result = await mediator.Send(command);

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("PhoneNumber");
    }

    [Fact]
    public async Task Register_WithShortPassword_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, "123");

        var result = await mediator.Send(command);

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("Password");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand(ValidPhoneNumber, "invalid-email", ValidFullName, ValidPassword);

        var result = await mediator.Send(command);

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Register_WithEmptyFullName_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, "", ValidPassword);

        var result = await mediator.Send(command);

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("FullName");
    }

    [Fact]
    public async Task Register_WithOnlyPhoneNumber_ShouldSucceed()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand(ValidPhoneNumber, null, ValidFullName, ValidPassword);

        var result = await mediator.Send(command);

        var response = result.ShouldBeSuccess();
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithOnlyEmail_ShouldSucceed()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand(null, ValidEmail, ValidFullName, ValidPassword);

        var result = await mediator.Send(command);

        var response = result.ShouldBeSuccess();
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithBothPhoneAndEmailNull_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand(null, null, ValidFullName, ValidPassword);

        var result = await mediator.Send(command);

        result.ShouldBeFailure();
    }
}