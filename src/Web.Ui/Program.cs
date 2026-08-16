using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web.Ui;
using Web.Ui.Configurations;
using Web.Ui.Services;
using AuthorizationMessageHandler = Web.Ui.Services.AuthorizationMessageHandler;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<ApiSettings>(options =>
    builder.Configuration.GetSection(ApiSettings.SectionName).Bind(options)
);
var apiSettings = builder.Configuration
                      .GetSection(ApiSettings.SectionName)
                      .Get<ApiSettings>()
                  ?? throw new InvalidOperationException("ApiSettings not configured");

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiSettings.BaseUrl);
    }).AddTypedClient<IApiClient>((httpClient, sp) =>
    {
        var apiClient = new ApiClient(httpClient)
        {
            ReadResponseAsString = true
        };
        return apiClient;
    })
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IClientLoggingService, ClientLoggingService>();

builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();