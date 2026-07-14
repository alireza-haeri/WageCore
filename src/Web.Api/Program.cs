const string webApplicationCorsPolicyName = "WebApplicationCors";

var builder = WebApplication.CreateBuilder(args);

//Logging
builder.Logging.ClearProviders();
builder.ConfigureSerilog();

//Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//Configure Application Settings
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection(nameof(ApplicationSettings)));
var applicationSettings = builder.Configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>()
                          ?? throw new InvalidOperationException(nameof(ApplicationSettings));

//Add Project Services

builder.Services.AddControllers();
//Add Authentication

//Add Cors Policies
builder.Services.AddCors(options =>
    options.AddPolicy(webApplicationCorsPolicyName, policy =>
        policy.WithOrigins(applicationSettings.CorsPolicy.Origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
    )
);

//Add Swagger
builder.ConfigureSwagger(applicationSettings.ApplicationName, applicationSettings.ApplicationVersion);

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger(applicationSettings.ApplicationName);

app.UseRouting();

app.UseCors(webApplicationCorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    //Migrations
    
    Log.Information("Starting web application...");
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}