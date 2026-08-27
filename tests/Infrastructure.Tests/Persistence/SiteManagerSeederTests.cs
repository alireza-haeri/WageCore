namespace Infrastructure.Tests.Persistence;

public class SiteManagerSeederTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SeedAsync_ShouldCreateSiteManagerUserAndRole()
    {
        await using var scope = fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SiteManagerSeeder>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var settings = TestApplicationSettings.Create().Value.SiteManager;

        await seeder.SeedAsync();

        var roleExists = await roleManager.RoleExistsAsync(ApplicationRoles.SiteManagerRule);
        roleExists.Should().BeTrue();

        var user = await userManager.FindByEmailAsync(settings.Email);
        user.Should().NotBeNull();
        user!.Email.Should().Be(settings.Email);
        user.FullName.Should().Be(settings.FullName);

        var isInRole = await userManager.IsInRoleAsync(user, ApplicationRoles.SiteManagerRule);
        isInRole.Should().BeTrue();

        var passwordValid = await userManager.CheckPasswordAsync(user, settings.Password);
        passwordValid.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_ShouldNotCreateDuplicateUser()
    {
        await using var scope = fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SiteManagerSeeder>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var settings = TestApplicationSettings.Create().Value.SiteManager;

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var users = userManager.Users.Where(x => x.Email == settings.Email).ToList();
        users.Should().ContainSingle();
    }

    [Fact]
    public async Task SeedAsync_WhenUserExistsWithoutRole_ShouldAssignRole()
    {
        await using var scope = fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SiteManagerSeeder>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var settings = TestApplicationSettings.Create().Value.SiteManager;

        var user = User.Create(null, settings.Email, settings.FullName).ShouldBeSuccess();
        var applicationUser = ApplicationUser.CrateFromUser(user);
        var createResult = await userManager.CreateAsync(applicationUser, settings.Password);
        createResult.Succeeded.Should().BeTrue();

        await seeder.SeedAsync();

        var storedUser = await userManager.FindByEmailAsync(settings.Email);
        storedUser.Should().NotBeNull();
        var isInRole = await userManager.IsInRoleAsync(storedUser!, ApplicationRoles.SiteManagerRule);
        isInRole.Should().BeTrue();
    }
}
