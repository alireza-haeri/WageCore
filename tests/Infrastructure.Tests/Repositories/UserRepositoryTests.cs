namespace Infrastructure.Tests.Repositories;

public class UserRepositoryTests(WageCoreDbContextFixture fixture) : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private const string ValidPhone = "09123456789";
    private const string ValidEmail = "ali@gmail.com";
    private const string ValidPassword = "Pass123456";
    private const string ValidFullName = "علی رضایی";

    private readonly UserBuilder _userBuilder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_WithValidUserAndPassword_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(user, ValidPassword);

        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithShortPassword_ShouldFail()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(user, "123");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_DuplicatePhoneNumber_ShouldFail()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user1 = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail("user1@example.com")
            .WithFullName(ValidFullName)
            .WithId(Guid.NewGuid())
            .CreateResult()
            .ShouldBeSuccess();

        var user2 = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail("user2@example.com")
            .WithFullName(ValidFullName)
            .WithId(Guid.NewGuid())
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user1, ValidPassword);
        var result = await repository.CreateAsync(user2, ValidPassword);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithOnlyPhoneNumber_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(null)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(user, ValidPassword);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithOnlyEmail_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(null)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(user, ValidPassword);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_WhenUserExistsWithPhoneNumber_ShouldReturnUser()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.GetAsync(ValidPhone, null);

        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be(ValidPhone);
        result.Email.Should().Be(ValidEmail);
        result.FullName.Should().Be(ValidFullName);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetAsync_WhenUserExistsWithEmail_ShouldReturnUser()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.GetAsync(null, ValidEmail);

        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be(ValidPhone);
        result.Email.Should().Be(ValidEmail);
        result.FullName.Should().Be(ValidFullName);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetAsync_WhenUserExistsWithOnlyEmail_ShouldReturnUser()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(null)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.GetAsync(null, ValidEmail);

        result.Should().NotBeNull();
        result!.Email.Should().Be(ValidEmail);
        result.PhoneNumber.Should().BeNull();
        result.FullName.Should().Be(ValidFullName);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var result = await repository.GetAsync("09999999999", null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckPasswordAsync_WithCorrectPassword_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.CheckPasswordAsync(user, ValidPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPasswordAsync_WithWrongPassword_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.CheckPasswordAsync(user, "WrongPassword");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPasswordAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var nonExistentUser = _userBuilder
            .WithPhoneNumber("09999999999")
            .WithEmail("nonexistent@example.com")
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CheckPasswordAsync(nonExistentUser, ValidPassword);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPasswordAsync_ForUserWithOnlyEmail_ShouldWork()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(null)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.CheckPasswordAsync(user, ValidPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserExistsWithPhoneNumber_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.ExistsAsync(ValidPhone, null);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserExistsWithEmail_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.ExistsAsync(null, ValidEmail);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetRolesAsync_WhenUserHasRole_ShouldReturnRoles()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);
        await roleManager.CreateAsync(new IdentityRole<Guid>(ApplicationRoles.SiteManagerRule));
        var applicationUser = await userManager.FindByIdAsync(user.Id.ToString());
        await userManager.AddToRoleAsync(applicationUser!, ApplicationRoles.SiteManagerRule);

        var result = await repository.GetRolesAsync(user.Id);

        result.Should().Contain(ApplicationRoles.SiteManagerRule);
    }

    [Fact]
    public async Task GetRolesAsync_WhenUserHasNoRole_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = _userBuilder
            .WithPhoneNumber(ValidPhone)
            .WithEmail(ValidEmail)
            .WithFullName(ValidFullName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(user, ValidPassword);

        var result = await repository.GetRolesAsync(user.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var result = await repository.ExistsAsync("09999999999", "nonexistent@example.com");

        result.Should().BeFalse();
    }
}