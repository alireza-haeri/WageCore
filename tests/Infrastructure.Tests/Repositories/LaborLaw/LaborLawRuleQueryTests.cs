namespace Infrastructure.Tests.Repositories.LaborLaw;

public class LaborLawRuleQueryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private readonly LaborLawRuleItemBuilder _builder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<LaborLawRuleItem> CreateRuleAsync(
        AsyncServiceScope scope,
        decimal value,
        DateOnly effectiveFrom)
    {
        var repository = scope.ServiceProvider.GetRequiredService<LaborLawRuleRepository>();
        var rule = _builder
            .WithId(Guid.NewGuid())
            .WithKey(LaborLawRuleKey.MinimumDailySalary)
            .WithValue(value)
            .WithEffectiveFrom(effectiveFrom)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(rule);
        result.Should().Be(rule.Id);

        return rule;
    }

    [Fact]
    public async Task GetActiveValueAsync_WhenNoRuleExists_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        var result = await query.GetActiveValueAsync(
            LaborLawRuleKey.MinimumDailySalary,
            DateOnly.FromDateTime(DateTime.Now));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveValueAsync_WhenDateIsBeforeAllRules_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        await CreateRuleAsync(scope, 71_661_840m, DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        var result = await query.GetActiveValueAsync(
            LaborLawRuleKey.MinimumDailySalary,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-20)));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveValueAsync_ShouldReturnLatestRuleNotAfterDate()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        var olderDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-40));
        var newerDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));

        await CreateRuleAsync(scope, 71_661_840m, olderDate);
        await CreateRuleAsync(scope, 103_909_680m, newerDate);

        var olderValue = await query.GetActiveValueAsync(
            LaborLawRuleKey.MinimumDailySalary,
            olderDate.AddDays(1));
        var newerValue = await query.GetActiveValueAsync(
            LaborLawRuleKey.MinimumDailySalary,
            DateOnly.FromDateTime(DateTime.Now));

        olderValue.Should().Be(71_661_840m);
        newerValue.Should().Be(103_909_680m);
    }

    [Fact]
    public async Task GetLaborLawRulesAsync_WithoutFilter_ShouldReturnAllRules()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        await CreateRuleAsync(scope, 71_661_840m, DateOnly.FromDateTime(DateTime.Now.AddDays(-40)));
        await CreateRuleAsync(scope, 103_909_680m, DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        var result = await query.GetLaborLawRulesAsync(new PaginationDto(1, 10));

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Value.Should().Be(103_909_680m);
        result.Items[1].Value.Should().Be(71_661_840m);
    }

    [Fact]
    public async Task GetLaborLawRulesAsync_WithKeyFilter_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        await CreateRuleAsync(scope, 71_661_840m, DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        var result = await query.GetLaborLawRulesAsync(
            new PaginationDto(1, 10),
            LaborLawRuleKey.MinimumDailySalary);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Key.Should().Be(LaborLawRuleKey.MinimumDailySalary);
    }

    [Fact]
    public async Task GetLaborLawRulesAsync_WithPagination_ShouldPaginateCorrectly()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        await CreateRuleAsync(scope, 10_000_000m, DateOnly.FromDateTime(DateTime.Now.AddDays(-50)));
        await CreateRuleAsync(scope, 20_000_000m, DateOnly.FromDateTime(DateTime.Now.AddDays(-40)));
        await CreateRuleAsync(scope, 30_000_000m, DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));

        var result = await query.GetLaborLawRulesAsync(new PaginationDto(1, 2));

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLaborLawRuleByIdAsync_WhenRuleExists_ShouldReturnRule()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var rule = await CreateRuleAsync(scope, 103_909_680m, effectiveFrom);

        var result = await query.GetLaborLawRuleByIdAsync(rule.Id);

        result.Should().NotBeNull();
        result!.Key.Should().Be(LaborLawRuleKey.MinimumDailySalary);
        result.Value.Should().Be(103_909_680m);
        result.EffectiveFrom.Should().Be(effectiveFrom);
    }

    [Fact]
    public async Task IsExistEffectiveFrom_WhenDateExists_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));

        await CreateRuleAsync(scope, 71_661_840m, effectiveFrom);

        var result = await query.IsExistEffectiveFrom(LaborLawRuleKey.MinimumDailySalary, effectiveFrom);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistEffectiveFrom_WhenDateDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        await CreateRuleAsync(scope, 71_661_840m, DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));

        var result = await query.IsExistEffectiveFrom(
            LaborLawRuleKey.MinimumDailySalary,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistEffectiveFrom_WithExcludeRuleId_ShouldIgnoreThatRule()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
        var rule = await CreateRuleAsync(scope, 71_661_840m, effectiveFrom);

        var result = await query.IsExistEffectiveFrom(LaborLawRuleKey.MinimumDailySalary, effectiveFrom, rule.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistEffectiveFrom_WhenDateExistsForAnotherKey_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));

        await CreateRuleAsync(scope, 71_661_840m, effectiveFrom);

        var result = await query.IsExistEffectiveFrom(LaborLawRuleKey.DailyWorkingHours, effectiveFrom);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetLaborLawRuleByIdAsync_WhenRuleDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        var result = await query.GetLaborLawRuleByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
