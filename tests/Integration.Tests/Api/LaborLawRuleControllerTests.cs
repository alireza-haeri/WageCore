using System.Net.Http.Headers;
using Application.Features.LaborLawRules;
using Core.Domain.Enums;
using Web.Api.Controllers.Users.Contracts;

namespace Integration.Tests.Api;

public class LaborLawRuleControllerTests(ApiFixture fixture) : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = fixture.CreateClient();

    private const string LaborLawRulesUrl = "/api/v1/labor-law-rules";
    private const string RegisterUrl = "/api/v1/users/register";
    private const string LoginUrl = "/api/v1/users/login";

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetLaborLawRules_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync($"{LaborLawRulesUrl}?Pagination.PageNumber=1&Pagination.PageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLaborLawRules_WithUserWithoutSiteManagerRule_ShouldReturnForbidden()
    {
        var registerRequest = new RegisterUserCommand("09123456789", "ali@gmail.com", "علی رضایی", "123456");
        var registerResponse = await _client.PostAsJsonAsync(RegisterUrl, registerRequest);
        var registerResult = await registerResponse.ShouldBeSuccess<RegisterUserCommandResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResult.Token);

        var response = await _client.GetAsync($"{LaborLawRulesUrl}?Pagination.PageNumber=1&Pagination.PageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateLaborLawRule_WithSiteManagerRule_ShouldReturnOk()
    {
        await SeedSiteManagerAsync();
        await LoginAsSiteManagerAsync();

        var response = await _client.PostAsJsonAsync(LaborLawRulesUrl, new
        {
            laborLawRuleKey = LaborLawRuleKey.MinimumMonthlySalary,
            value = 71_661_840m,
            effectiveFrom = "1403/01/01"
        });
        var result = await response.ShouldBeSuccess<CreateLaborLawRuleCommandResponse>();

        result.LaborLawRuleId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateLaborLawRule_WithUserWithoutSiteManagerRule_ShouldReturnForbidden()
    {
        var registerRequest = new RegisterUserCommand("09123456789", "ali@gmail.com", "علی رضایی", "123456");
        var registerResponse = await _client.PostAsJsonAsync(RegisterUrl, registerRequest);
        var registerResult = await registerResponse.ShouldBeSuccess<RegisterUserCommandResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResult.Token);

        var response = await _client.PostAsJsonAsync(LaborLawRulesUrl, new
        {
            laborLawRuleKey = LaborLawRuleKey.MinimumMonthlySalary,
            value = 71_661_840m,
            effectiveFrom = "1403/01/01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task SeedSiteManagerAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SiteManagerSeeder>();
        await seeder.SeedAsync();
    }

    private async Task LoginAsSiteManagerAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var appSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApplicationSettings>>();
        var email = appSettings.Value.SiteManager.Email;
        var password = appSettings.Value.SiteManager.Password;

        var loginResponse = await _client.PostAsJsonAsync(LoginUrl, new LoginUserRequest(null, email, password));
        var loginResult = await loginResponse.ShouldBeSuccess<LoginUserCommandResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);
    }
}
