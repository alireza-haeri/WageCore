namespace Integration.Tests.Fixtures;

public class ApiFixture : WebApplicationFactory<Program>
{
    // یک نام یکتا برای هر instance از ApiFixture
    private readonly string _dbName = $"WageCoreInMemoryDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<WageCoreDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<WageCoreDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}