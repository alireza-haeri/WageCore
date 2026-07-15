

namespace Infrastructure.Tests.Fixtures;

public sealed class WageCoreDbContextFixture : IDisposable
{
    private readonly string _databaseName = $"IdentityTestDb_{Guid.NewGuid()}";

    private ServiceProvider RootProvider { get; }

    public WageCoreDbContextFixture()
    {
        var services = new ServiceCollection();

        services.AddDbContext<WageCoreDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<WageCoreDbContext>()
            .AddDefaultTokenProviders();

        services.AddLogging();
        services.AddScoped<UserRepository>();

        RootProvider = services.BuildServiceProvider();
    }

    public AsyncServiceScope CreateScope()
    {
        return RootProvider.CreateAsyncScope();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public void Dispose()
    {
        RootProvider.Dispose();
    }
}