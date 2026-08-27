namespace Application.Tests.Features.CalculationFormulas.Command.UpdateCalculationFormula;

public class UpdateCalculationFormulaCommandValidatorTests
{
    private readonly UpdateCalculationFormulaCommandValidator _validator = new();

    private static readonly Guid ValidFormulaId = Guid.NewGuid();
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const string ValidExpression = "OvertimeHours * HourlyRate * 1.5";

    private static UpdateCalculationFormulaCommand CreateValidCommand(
        Guid? formulaId = null,
        FormulaKey? key = FormulaKey.OvertimePay,
        string expression = ValidExpression,
        DateOnly? effectiveFrom = null) =>
        new(formulaId ?? ValidFormulaId, key, expression, effectiveFrom ?? ValidEffectiveFrom);

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyFormulaId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(formulaId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CalculationFormulaId);
    }

    [Fact]
    public void Validate_WithNullKey_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(key: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceExpression_ShouldHaveValidationError(string? expression)
    {
        var command = CreateValidCommand(expression: expression!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Expression);
    }

    [Fact]
    public void Validate_WithNullEffectiveFrom_ShouldHaveValidationError()
    {
        var command = new UpdateCalculationFormulaCommand(
            ValidFormulaId,
            FormulaKey.OvertimePay,
            ValidExpression,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EffectiveFrom);
    }
}
