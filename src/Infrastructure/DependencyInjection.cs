namespace Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder ConfigureInfrastructure(this WebApplicationBuilder builder)
    {
        var applicationSettings = builder.GetApplicationSettings();
        var databaseSettings = applicationSettings.Databases;

        builder.Services.AddDbContext<WageCoreDbContext>(options =>
        {
            options.UseSqlServer(databaseSettings.ConnectionString);
        });

        // Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<WageCoreDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        return builder;
    }

    public static async Task MigrateDatabaseAsync(this WebApplication app)

    {
        if (!app.Environment.IsEnvironment("Testing"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }

    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSettings = builder.GetApplicationSettings().JwtToken;

        var key = Encoding.UTF8.GetBytes(jwtSettings.SigningKey);

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}