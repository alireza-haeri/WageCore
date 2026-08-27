namespace Application.Tests.Features.CalculationFormulas.Command.UpdateCalculationFormula;

public class UpdateCalculationFormulaCommandHandlerTests
{
    private readonly ICalculationFormulaRepository _calculationFormulaRepository;
    private readonly ICalculationFormulaQuery _calculationFormulaQuery;
    private readonly UpdateCalculationFormulaCommandHandler _handler;
    private readonly CalculationFormulaBuilder _builder;

    private static readonly Guid ValidFormulaId = Guid.NewGuid();
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const string ValidExpression = "OvertimeHours * HourlyRate * 1.5";

    public UpdateCalculationFormulaCommandHandlerTests()
    {
        _calculationFormulaRepository = Substitute.For<ICalculationFormulaRepository>();
        _calculationFormulaQuery = Substitute.For<ICalculationFormulaQuery>();
        _handler = new UpdateCalculationFormulaCommandHandler(
            _calculationFormulaRepository,
            _calculationFormulaQuery);
        _builder = new CalculationFormulaBuilder();
    }

    private CalculationFormula CreateValidFormula()
    {
        return _builder
            .WithId(ValidFormulaId)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private static UpdateCalculationFormulaCommand CreateValidCommand(
        Guid? formulaId = null,
        FormulaKey? key = FormulaKey.OvertimePay,
        string expression = ValidExpression,
        DateOnly? effectiveFrom = null) =>
        new(formulaId ?? ValidFormulaId, key, expression, effectiveFrom ?? ValidEffectiveFrom);

    private void SetupNoDuplicateEffectiveFrom()
    {
        _calculationFormulaQuery.IsExistEffectiveFrom(
                Arg.Any<DateOnly>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateFormulaAndReturnTrue()
    {
        var command = CreateValidCommand();
        var formula = CreateValidFormula();

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(formula);
        SetupNoDuplicateEffectiveFrom();
        _calculationFormulaRepository.UpdateAsync(formula, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            formula.Expression.Should().Be(ValidExpression);
            formula.EffectiveFrom.Should().Be(ValidEffectiveFrom);
        }
    }

    [Fact]
    public async Task Handle_WhenFormulaNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns((CalculationFormula?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
        await _calculationFormulaRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromAlreadyExists_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var formula = CreateValidFormula();

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(formula);
        _calculationFormulaQuery.IsExistEffectiveFrom(
                ValidEffectiveFrom,
                ValidFormulaId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اجرا تکراری است.", BadResultType.Validation);
        await _calculationFormulaRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(expression: " ");
        var formula = CreateValidFormula();

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(formula);
        SetupNoDuplicateEffectiveFrom();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("عبارت فرمول نمیتواند خالی باشد.", BadResultType.General);
        await _calculationFormulaRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var formula = CreateValidFormula();

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(formula);
        SetupNoDuplicateEffectiveFrom();
        _calculationFormulaRepository.UpdateAsync(formula, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
