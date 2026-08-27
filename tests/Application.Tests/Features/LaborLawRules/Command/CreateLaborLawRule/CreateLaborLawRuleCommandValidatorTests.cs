namespace Application.Tests.Features.LaborLawRules.Command.CreateLaborLawRule;

public class CreateLaborLawRuleCommandValidatorTests
{
    private readonly CreateLaborLawRuleCommandValidator _validator = new();

    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const decimal ValidValue = 71_661_840m;

    private static CreateLaborLawRuleCommand CreateValidCommand(
        LaborLawRuleKey? key = LaborLawRuleKey.MinimumMonthlySalary,
        decimal? value = ValidValue,
        DateOnly? effectiveFrom = null) =>
        new(key, value, effectiveFrom ?? ValidEffectiveFrom);

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithZeroValue_ShouldNotHaveAnyErrors()
    {
        var command = CreateValidCommand(value: 0);

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
        var command = CreateValidCommand(key: (LaborLawRuleKey)99);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_WithNullValue_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(value: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_WithNegativeValue_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(value: -1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Value)
            .WithErrorMessage("مقدار قانون نمیتواند منفی باشد.");
    }

    [Fact]
    public void Validate_WithNullEffectiveFrom_ShouldHaveValidationError()
    {
        var command = new CreateLaborLawRuleCommand(
            LaborLawRuleKey.MinimumMonthlySalary,
            ValidValue,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EffectiveFrom);
    }
}
