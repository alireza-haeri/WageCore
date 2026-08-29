using Infrastructure.Persistence.Dapper;

namespace Integration.Tests.Fixtures;

public class ApiFixture : WebApplicationFactory<Program>
{
    private static readonly MsSqlContainer SqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
        .Build();

    private readonly string _dbName = $"WageCoreTestDb_{Guid.NewGuid():N}";
    private string _connectionString = null!;
    private Respawner _respawner = null!;

    protected override async Task InitializeAsync()
    {
        if (SqlContainer.State != TestcontainersStates.Running)
        {
            await SqlContainer.StartAsync();
        }

        _connectionString = new SqlConnectionStringBuilder(SqlContainer.GetConnectionString())
        {
            InitialCatalog = _dbName
        }.ConnectionString;

        await base.InitializeAsync();

        await using (var scope = Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<WageCoreDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            var connectionFactoryDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDbConnectionFactory));
            if (connectionFactoryDescriptor != null)
                services.Remove(connectionFactoryDescriptor);

            services.AddDbContext<WageCoreDbContext>(options =>
                options.UseSqlServer(_connectionString));

            services.AddSingleton<IDbConnectionFactory>(
                new TestSqlConnectionFactory(_connectionString));

            services.AddControllers()
                .AddApplicationPart(typeof(TestDateTimeController).Assembly);
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    private sealed class TestSqlConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public IDbConnection CreateConnection() => new SqlConnection(connectionString);
    }
}
