using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Tests.Fixtures;

public sealed class WageCoreDbContextFixture : IAsyncLifetime
{
    private static readonly MsSqlContainer SqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
        .Build();

    private readonly string _databaseName = $"TestDb_{Guid.NewGuid():N}";
    private ServiceProvider _serviceProvider = null!;
    private string _connectionString = null!;
    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        if (SqlContainer.State != TestcontainersStates.Running)
        {
            await SqlContainer.StartAsync();
        }

        var baseConnectionString = SqlContainer.GetConnectionString();
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = _databaseName
        };
        _connectionString = builder.ConnectionString;

        var services = new ServiceCollection();

        services.AddDbContext<WageCoreDbContext>(options =>
            options.UseSqlServer(_connectionString));

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

        DapperTypeHandlers.Register();
        services.AddSingleton<IDbConnectionFactory>(
            new TestSqlConnectionFactory(_connectionString));

        services.AddLogging();
        services.AddSingleton(TestApplicationSettings.Create());
        services.AddScoped<SiteManagerSeeder>();
        services.AddScoped<UserRepository>();
        services.AddScoped<WorkshopRepository>();
        services.AddScoped<EmployeeRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IWorkshopQuery, WorkshopQuery>();
        services.AddScoped<IDepartmentQuery, DepartmentQuery>();
        services.AddScoped<IEmployeeQuery, EmployeeQuery>();
        services.AddScoped<SalaryDecreeRepository>();
        services.AddScoped<ISalaryDecreeRepository, SalaryDecreeRepository>();
        services.AddScoped<SalaryDecreeQuery>();
        services.AddScoped<ISalaryDecreeQuery, SalaryDecreeQuery>();
        services.AddScoped<LaborLawRuleRepository>();
        services.AddScoped<ILaborLawRuleRepository, LaborLawRuleRepository>();
        services.AddScoped<ILaborLawRuleQuery, LaborLawRuleQuery>();
        services.AddScoped<CalculationFormulaRepository>();
        services.AddScoped<ICalculationFormulaRepository, CalculationFormulaRepository>();
        services.AddScoped<ICalculationFormulaQuery, CalculationFormulaQuery>();

        _serviceProvider = services.BuildServiceProvider();

        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();
            // Migrations are intentionally left untouched for this domain change, so tests
            // create the schema directly from the current model.
            await dbContext.Database.EnsureCreatedAsync();
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
        });
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }
    
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public AsyncServiceScope CreateScope() => _serviceProvider.CreateAsyncScope();

    private sealed class TestSqlConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public IDbConnection CreateConnection() => new SqlConnection(connectionString);
    }
}