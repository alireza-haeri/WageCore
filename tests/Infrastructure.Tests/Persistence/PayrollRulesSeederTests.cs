namespace Infrastructure.Tests.Persistence;

public class PayrollRulesSeederTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    // 1400-01-01, the seed date (Farvardin 1 of the seeded Persian year)
    private static readonly DateOnly EffectiveFrom = new(2021, 3, 21);

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SeedAsync_ShouldCreateEveryRuleAndFormulaForTheSeedYear()
    {
        await using var scope = fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<PayrollRulesSeeder>();
        var laborLawRuleQuery = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();
        var calculationFormulaQuery = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        await seeder.SeedAsync();

        foreach (var (key, value) in PayrollFormulaCatalog.RuleValues)
        {
            var active = await laborLawRuleQuery.GetActiveValueAsync(key, EffectiveFrom);
            active.Should().Be(value);
        }

        foreach (var (key, expression) in PayrollFormulaCatalog.Formulas)
        {
            var active = await calculationFormulaQuery.GetActiveExpressionAsync(key, EffectiveFrom);
            active.Should().Be(expression);
        }
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_ShouldNotCreateDuplicates()
    {
        await using var scope = fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<PayrollRulesSeeder>();
        var laborLawRuleQuery = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();
        var calculationFormulaQuery = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        foreach (var key in PayrollFormulaCatalog.RuleValues.Keys)
        {
            var rules = await laborLawRuleQuery.GetLaborLawRulesAsync(new PaginationDto(1, 100), key);
            rules.TotalCount.Should().Be(1);
        }

        foreach (var key in PayrollFormulaCatalog.Formulas.Keys)
        {
            var formulas = await calculationFormulaQuery.GetCalculationFormulasAsync(new PaginationDto(1, 100), key);
            formulas.TotalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SeedAsync_ShouldKeepManuallyAddedRulesForOtherDatesIntact()
    {
        await using var scope = fixture.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<PayrollRulesSeeder>();
        var repository = scope.ServiceProvider.GetRequiredService<ILaborLawRuleRepository>();
        var laborLawRuleQuery = scope.ServiceProvider.GetRequiredService<ILaborLawRuleQuery>();

        var manualRule = LaborLawRuleItem.Create(
            LaborLawRuleKey.MinimumDailySalary,
            PayrollFormulaCatalog.MinimumDailySalary + 1,
            new DateOnly(2022, 3, 21)).ShouldBeSuccess();
        var createdId = await repository.CreateAsync(manualRule);
        createdId.Should().Be(manualRule.Id);

        await seeder.SeedAsync();

        var activeBeforeSeedDate = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MinimumDailySalary, new DateOnly(2021, 1, 1));
        activeBeforeSeedDate.Should().Be(null);

        var activeOnSeedDate = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MinimumDailySalary, EffectiveFrom);
        activeOnSeedDate.Should().Be(PayrollFormulaCatalog.MinimumDailySalary);

        var activeAfterSeedDate = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MinimumDailySalary, new DateOnly(2022, 6, 1));
        activeAfterSeedDate.Should().Be(manualRule.Value);
    }
}
