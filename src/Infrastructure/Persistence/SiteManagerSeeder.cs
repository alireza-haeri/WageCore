namespace Infrastructure.Persistence;

public class SiteManagerSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOptions<ApplicationSettings> options,
    ILogger<SiteManagerSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value.SiteManager;

        if (!await roleManager.RoleExistsAsync(ApplicationRoles.SiteManagerRule))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(ApplicationRoles.SiteManagerRule));
            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to create {Role} role.", ApplicationRoles.SiteManagerRule);
                throw new InvalidOperationException("خطا در ایجاد نقش مدیر سایت.");
            }
        }

        var existingUser = await userManager.FindByEmailAsync(settings.Email);
        if (existingUser is null)
        {
            var userResult = User.Create(null, settings.Email, settings.FullName);
            if (!userResult.IsSuccess)
            {
                logger.LogError("Failed to create site manager domain user: {Error}", userResult.ErrorMessage);
                throw new InvalidOperationException(userResult.ErrorMessage);
            }

            var applicationUser = ApplicationUser.CrateFromUser(userResult.Response!);
            var createResult = await userManager.CreateAsync(applicationUser, settings.Password);
            if (!createResult.Succeeded)
            {
                logger.LogError("Failed to create site manager user.");
                throw new InvalidOperationException("خطا در ایجاد کاربر مدیر سایت.");
            }

            existingUser = applicationUser;
        }

        if (!await userManager.IsInRoleAsync(existingUser, ApplicationRoles.SiteManagerRule))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(existingUser, ApplicationRoles.SiteManagerRule);
            if (!addToRoleResult.Succeeded)
            {
                logger.LogError("Failed to assign {Role} to site manager.", ApplicationRoles.SiteManagerRule);
                throw new InvalidOperationException("خطا در اختصاص نقش مدیر سایت.");
            }
        }
    }
}
