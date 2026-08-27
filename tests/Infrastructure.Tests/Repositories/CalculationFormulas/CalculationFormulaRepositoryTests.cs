namespace Infrastructure.Tests.Repositories.CalculationFormulas;

public class CalculationFormulaRepositoryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private readonly CalculationFormulaBuilder _builder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<CalculationFormula> CreateFormulaAsync(
        AsyncServiceScope scope,
        string expression = "OvertimeHours * HourlyRate * 1.4",
        DateOnly? effectiveFrom = null)
    {
        var repository = scope.ServiceProvider.GetRequiredService<CalculationFormulaRepository>();
        var formula = _builder
            .WithId(Guid.NewGuid())
            .WithKey(FormulaKey.OvertimePay)
            .WithExpression(expression)
            .WithEffectiveFrom(effectiveFrom ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(formula);
        result.Should().Be(formula.Id);

        return formula;
    }

    [Fact]
    public async Task CreateAsync_WithValidFormula_ShouldPersistFormula()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<CalculationFormulaRepository>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));

        var formula = _builder
            .WithId(Guid.NewGuid())
            .WithExpression("Hours * Rate * 1.5")
            .WithEffectiveFrom(effectiveFrom)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(formula);

        result.Should().Be(formula.Id);

        var stored = await repository.GetByIdAsync(formula.Id);
        stored.Should().NotBeNull();
        stored!.Key.Should().Be(FormulaKey.OvertimePay);
        stored.Expression.Should().Be("Hours * Rate * 1.5");
        stored.EffectiveFrom.Should().Be(effectiveFrom);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFormulaDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<CalculationFormulaRepository>();

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenFormulaExists_ShouldPersistChanges()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<CalculationFormulaRepository>();
        var formula = await CreateFormulaAsync(scope);
        var newEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        formula.Update(FormulaKey.OvertimePay, "Hours * Rate * 1.5", newEffectiveFrom).ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(formula);

        updateResult.Should().BeTrue();

        var stored = await repository.GetByIdAsync(formula.Id);
        stored.Should().NotBeNull();
        stored!.Expression.Should().Be("Hours * Rate * 1.5");
        stored.EffectiveFrom.Should().Be(newEffectiveFrom);
    }

    [Fact]
    public async Task DeleteAsync_WhenFormulaExists_ShouldDeleteFormula()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<CalculationFormulaRepository>();
        var formula = await CreateFormulaAsync(scope);

        var deleteResult = await repository.DeleteAsync(formula.Id);

        deleteResult.Should().BeTrue();

        var stored = await repository.GetByIdAsync(formula.Id);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenFormulaDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<CalculationFormulaRepository>();

        var deleteResult = await repository.DeleteAsync(Guid.NewGuid());

        deleteResult.Should().BeFalse();
    }
}
