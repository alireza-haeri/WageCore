namespace Infrastructure.Tests.Repositories.CalculationFormulas;

public class CalculationFormulaQueryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private readonly CalculationFormulaBuilder _builder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<CalculationFormula> CreateFormulaAsync(
        AsyncServiceScope scope,
        string expression,
        DateOnly effectiveFrom)
    {
        var repository = scope.ServiceProvider.GetRequiredService<CalculationFormulaRepository>();
        var formula = _builder
            .WithId(Guid.NewGuid())
            .WithKey(FormulaKey.OvertimePay)
            .WithExpression(expression)
            .WithEffectiveFrom(effectiveFrom)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(formula);
        result.Should().Be(formula.Id);

        return formula;
    }

    [Fact]
    public async Task GetActiveExpressionAsync_WhenNoFormulaExists_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        var result = await query.GetActiveExpressionAsync(
            FormulaKey.OvertimePay,
            DateOnly.FromDateTime(DateTime.Now));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveExpressionAsync_WhenDateIsBeforeAllFormulas_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        await CreateFormulaAsync(scope, "Hours * Rate * 1.4", DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        var result = await query.GetActiveExpressionAsync(
            FormulaKey.OvertimePay,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-20)));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveExpressionAsync_ShouldReturnLatestFormulaNotAfterDate()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        var olderDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-40));
        var newerDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));

        await CreateFormulaAsync(scope, "Hours * Rate * 1.4", olderDate);
        await CreateFormulaAsync(scope, "Hours * Rate * 1.5", newerDate);

        var olderExpression = await query.GetActiveExpressionAsync(
            FormulaKey.OvertimePay,
            olderDate.AddDays(1));
        var newerExpression = await query.GetActiveExpressionAsync(
            FormulaKey.OvertimePay,
            DateOnly.FromDateTime(DateTime.Now));

        olderExpression.Should().Be("Hours * Rate * 1.4");
        newerExpression.Should().Be("Hours * Rate * 1.5");
    }

    [Fact]
    public async Task GetCalculationFormulasAsync_WithoutFilter_ShouldReturnAllFormulas()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        await CreateFormulaAsync(scope, "Hours * Rate * 1.4", DateOnly.FromDateTime(DateTime.Now.AddDays(-40)));
        await CreateFormulaAsync(scope, "Hours * Rate * 1.5", DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        var result = await query.GetCalculationFormulasAsync(new PaginationDto(1, 10));

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Expression.Should().Be("Hours * Rate * 1.5");
        result.Items[1].Expression.Should().Be("Hours * Rate * 1.4");
    }

    [Fact]
    public async Task GetCalculationFormulasAsync_WithKeyFilter_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        await CreateFormulaAsync(scope, "Hours * Rate * 1.4", DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        var result = await query.GetCalculationFormulasAsync(
            new PaginationDto(1, 10),
            FormulaKey.OvertimePay);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Key.Should().Be(FormulaKey.OvertimePay);
    }

    [Fact]
    public async Task GetCalculationFormulasAsync_WithPagination_ShouldPaginateCorrectly()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        await CreateFormulaAsync(scope, "A", DateOnly.FromDateTime(DateTime.Now.AddDays(-50)));
        await CreateFormulaAsync(scope, "B", DateOnly.FromDateTime(DateTime.Now.AddDays(-40)));
        await CreateFormulaAsync(scope, "C", DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));

        var result = await query.GetCalculationFormulasAsync(new PaginationDto(1, 2));

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCalculationFormulaByIdAsync_WhenFormulaExists_ShouldReturnFormula()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var formula = await CreateFormulaAsync(scope, "Hours * Rate * 1.5", effectiveFrom);

        var result = await query.GetCalculationFormulaByIdAsync(formula.Id);

        result.Should().NotBeNull();
        result!.Key.Should().Be(FormulaKey.OvertimePay);
        result.Expression.Should().Be("Hours * Rate * 1.5");
        result.EffectiveFrom.Should().Be(effectiveFrom);
    }

    [Fact]
    public async Task GetCalculationFormulaByIdAsync_WhenFormulaDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        var result = await query.GetCalculationFormulaByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task IsExistEffectiveFrom_WhenDateExists_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));

        await CreateFormulaAsync(scope, "Hours * Rate * 1.4", effectiveFrom);

        var result = await query.IsExistEffectiveFrom(effectiveFrom);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistEffectiveFrom_WhenDateDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();

        await CreateFormulaAsync(scope, "Hours * Rate * 1.4", DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));

        var result = await query.IsExistEffectiveFrom(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistEffectiveFrom_WithExcludeFormulaId_ShouldIgnoreThatFormula()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ICalculationFormulaQuery>();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
        var formula = await CreateFormulaAsync(scope, "Hours * Rate * 1.4", effectiveFrom);

        var result = await query.IsExistEffectiveFrom(effectiveFrom, formula.Id);

        result.Should().BeFalse();
    }
}
