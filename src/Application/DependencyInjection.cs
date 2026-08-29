namespace Application;

public static class DependencyInjection
{
    public static WebApplicationBuilder ConfigureCore(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(options =>
            {
                options.Lifetime = ServiceLifetime.Scoped;
                options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            })
            .AddGlobalBehaviors();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddScoped<IPayrollCalculationService, PayrollCalculationService>();
        builder.Services.AddScoped<IPayrollLimitsResolver, PayrollLimitsResolver>();

        return builder;
    }
}