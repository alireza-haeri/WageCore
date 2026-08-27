namespace Infrastructure.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<IdentityResult> CreateAsync(User user, string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var applicationUser = ApplicationUser.CrateFromUser(user);

            var result = await userManager.CreateAsync(applicationUser, password);

            return ToIdentityResult(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating user with phone number {PhoneNumber}", user.PhoneNumber);
            return new IdentityResult(false, new Dictionary<string, string[]>
                { { "General", ["خطا در هنگام ایجاد کاربر."] } }
            );
        }
    }

    public async Task<bool> CheckPasswordAsync(User user, string password,
        CancellationToken cancellationToken = default)
    {
        var applicationUser =
            await userManager.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.PhoneNumber == user.PhoneNumber || u.Email == user.Email,
                cancellationToken);
        if (applicationUser is null)
            return false;
        return await userManager.CheckPasswordAsync(applicationUser, password);
    }

    public async Task<User?> GetAsync(string? phoneNumber, string? email, CancellationToken cancellationToken = default)
    {
        var applicationUser = await userManager.Users
            .SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber || u.Email == email, cancellationToken);
        if (applicationUser is null)
            return null;

        return User.Create(applicationUser.Id, applicationUser.PhoneNumber!, applicationUser.Email,
            applicationUser.FullName).Response;
    }

    public async Task<bool> ExistsAsync(string? phoneNumber, string? email,
        CancellationToken cancellationToken = default)
    {
        return await userManager.Users.AsNoTracking()
            .AnyAsync(u => (phoneNumber != null && u.PhoneNumber == phoneNumber) ||
                           (email != null && u.Email == email), cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var applicationUser = await userManager.FindByIdAsync(userId.ToString());
        if (applicationUser is null)
            return [];

        var roles = await userManager.GetRolesAsync(applicationUser);
        return roles.ToList();
    }

    private IdentityResult ToIdentityResult(Microsoft.AspNetCore.Identity.IdentityResult result) =>
        new
        (
            result.Succeeded,
            result.Errors
                .GroupBy(g => g.Code)
                .Select(g => new
                {
                    g.Key, IdentityErrors = g
                        .ToArray()
                        .Select(a => a.Description)
                        .ToArray()
                })
                .ToDictionary(k => k.Key, e => e.IdentityErrors)
        );
}

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; private set; } = null!;

    public static ApplicationUser CrateFromUser(User user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            UserName = user.PhoneNumber ?? user.Email!
        };
}