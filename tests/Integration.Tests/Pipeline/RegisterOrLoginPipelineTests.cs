using SamarPlanner.Shared.Tests.Assertions;

namespace SamarPlanner.Identity.Integration.Tests.Pipeline;

public class RegisterOrLoginPipelineTests(IdentityApiFixture fixture)
    : IClassFixture<IdentityApiFixture>, IAsyncLifetime
{
    public async System.Threading.Tasks.Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public System.Threading.Tasks.Task DisposeAsync() => System.Threading.Tasks.Task.CompletedTask;

    [Fact]
    public async System.Threading.Tasks.Task RegisterNewUser_WithValidData_ShouldSucceed()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterOrLoginCommand("09123456789", "Pass123456");

        var result = await mediator.Send(command);

        var response = result.ShouldBeSuccess();
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpireInMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginExistingUser_WithCorrectPassword_ShouldSucceed()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new RegisterOrLoginCommand("09123456789", "Pass123456"));

        var result = await mediator.Send(new RegisterOrLoginCommand("09123456789", "Pass123456"));

        var response = result.ShouldBeSuccess();
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async System.Threading.Tasks.Task LoginExistingUser_WithWrongPassword_ShouldFail()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new RegisterOrLoginCommand("09123456789", "Pass123456"));

        var result = await mediator.Send(new RegisterOrLoginCommand("09123456789", "WrongPassword"));

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async System.Threading.Tasks.Task Register_WithInvalidPhoneNumber_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterOrLoginCommand("123", "Pass123456");

        var result = await mediator.Send(command);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async System.Threading.Tasks.Task Register_WithShortPassword_ShouldFailValidation()
    {
        using var scope = fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new RegisterOrLoginCommand("09123456789", "123");

        var result = await mediator.Send(command);

        result.ShouldBeFailure(null,BadResultType.Validation);
    }
}