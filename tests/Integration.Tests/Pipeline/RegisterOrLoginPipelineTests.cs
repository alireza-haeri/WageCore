namespace Integration.Tests.Pipeline;

public class RegisterOrLoginPipelineTests(ApiFixture fixture)
    : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private const string ValidPhoneNumber = "09123456789";
    private const string ValidPassword = "Pass123456";
    
    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterNewUser_WithValidData_ShouldSucceed()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword);

        var result = await mediator.Send(command);

        var response = result.ShouldBeSuccess();
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpireInMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LoginExistingUser_WithCorrectPassword_ShouldSucceed()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword));

        var result = await mediator.Send(new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword));

        var response = result.ShouldBeSuccess();
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginExistingUser_WithWrongPassword_ShouldFail()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword));

        var result = await mediator.Send(new RegisterOrLoginCommand(ValidPhoneNumber, "WrongPassword"));

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Register_WithInvalidPhoneNumber_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterOrLoginCommand("123", ValidPassword);

        var result = await mediator.Send(command);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Register_WithShortPassword_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterOrLoginCommand(ValidPhoneNumber, "123");

        var result = await mediator.Send(command);

        result.ShouldBeFailure(null,BadResultType.Validation);
    }
}