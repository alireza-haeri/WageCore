namespace Application.Tests.Features.CalculationFormulas.Command.CreateCalculationFormula;

public class CreateCalculationFormulaCommandValidatorTests
{
    private readonly CreateCalculationFormulaCommandValidator _validator = new();

    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const string ValidExpression = "OvertimeHours * HourlyRate * 1.4";

    private static CreateCalculationFormulaCommand CreateValidCommand(
        FormulaKey? key = FormulaKey.OvertimePay,
        string expression = ValidExpression,
        DateOnly? effectiveFrom = null) =>
        new(key, expression, effectiveFrom ?? ValidEffectiveFrom);

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullKey_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(key: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_WithInvalidKey_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(key: (FormulaKey)99);

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
        var command = new CreateCalculationFormulaCommand(
            FormulaKey.OvertimePay,
            ValidExpression,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EffectiveFrom);
    }
}
