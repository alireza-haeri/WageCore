using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests.Fixtures;

public sealed class IdentityFixture : IDisposable
{
    private readonly string _databaseName = $"IdentityTestDb_{Guid.NewGuid()}";

    private ServiceProvider RootProvider { get; }

    public IdentityFixture()
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
            .AddEntityFrameworkStores<IdentityDbContext>()
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
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public void Dispose()
    {
        RootProvider.Dispose();
    }
}