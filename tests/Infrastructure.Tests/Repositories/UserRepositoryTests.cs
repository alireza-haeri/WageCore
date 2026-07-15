using SamarPlanner.Identity.Core.Abstractions;
using SamarPlanner.Shared.Tests.Assertions;

namespace SamarPlanner.Identity.Infrastructure.Tests.Repositories;

public class UserRepositoryTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>, IAsyncLifetime
{
    private const string ValidPhone = "09123456789";
    private const string ValidPassword = "Pass123456";

    private readonly UserBuilder _userBuilder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_WithValidUserAndPassword_ShouldSucceed()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        
        var user = _userBuilder.CreateResult().ShouldBeSuccess();

        var result = await repository.CreateAsync(user, ValidPassword);

        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithShortPassword_ShouldFail()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        
        var user = _userBuilder.CreateResult().ShouldBeSuccess();

        var result = await repository.CreateAsync(user, "123");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_DuplicateUser_ShouldFail()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        
        var user1 = _userBuilder.WithPhoneNumber(ValidPhone).WithId(Guid.NewGuid()).CreateResult().ShouldBeSuccess();
        var user2 = _userBuilder.WithPhoneNumber(ValidPhone).WithId(Guid.NewGuid()).CreateResult().ShouldBeSuccess();

        await repository.CreateAsync(user1, ValidPassword);

        var result = await repository.CreateAsync(user2, ValidPassword);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_WhenUserExists_ShouldReturnUser()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        
        var user = _userBuilder.CreateResult().ShouldBeSuccess();
        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.GetAsync(ValidPhone);

        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be(ValidPhone);
    }

    [Fact]
    public async Task GetAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        

        var result = await repository.GetAsync("09999999999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnUserWithCorrectId()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        
        var user = _userBuilder.CreateResult().ShouldBeSuccess();
        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.GetAsync(ValidPhone);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task CheckPasswordAsync_WithCorrectPassword_ShouldReturnTrue()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        
        var user = _userBuilder.CreateResult().ShouldBeSuccess();
        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.CheckPasswordAsync(ValidPhone, ValidPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPasswordAsync_WithWrongPassword_ShouldReturnFalse()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        
        var user = _userBuilder.CreateResult().ShouldBeSuccess();
        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.CheckPasswordAsync(ValidPhone, "WrongPassword");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPasswordAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        await using var scope =  fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var result = await repository.CheckPasswordAsync("09999999999", ValidPassword);

        result.Should().BeFalse();
    }
}