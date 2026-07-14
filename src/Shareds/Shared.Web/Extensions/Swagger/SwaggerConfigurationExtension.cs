namespace Shared.Web.Extensions.Swagger;

public static class SwaggerConfigurationExtension
{
    public static WebApplicationBuilder ConfigureSwagger(this WebApplicationBuilder builder,string applicationName, string version)
    {
        builder.Services.AddEndpointsApiExplorer();
        
        
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = applicationName, Version = version });

            options.EnableAnnotations();
            options.UseOneOfForPolymorphism();
            options.UseInlineDefinitionsForEnums();
            options.UseAllOfToExtendReferenceSchemas();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter token without Bearer"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });

            options.SchemaFilter<EnumSchemaFilter>();
            options.OperationFilter<SummaryFromOperationIdFilter>();
        });
        
        return builder;
    }
    
    public static WebApplication UseSwagger(this WebApplication app, string applicationName)
    {
        app.MapSwagger();
        app.MapScalarApiReference(options =>
        {
            options.Title = applicationName;
            options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
            
            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecuritySchemes = ["Bearer"]
            };

            options.OperationTitleSource = OperationTitleSource.Summary;
            options.Layout = ScalarLayout.Classic;
            options.Theme = ScalarTheme.BluePlanet;
            options.HiddenClients = true;
        });

        return app;
    }
}