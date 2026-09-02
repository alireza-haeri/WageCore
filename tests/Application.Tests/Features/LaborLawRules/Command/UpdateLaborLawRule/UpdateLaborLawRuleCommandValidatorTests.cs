namespace Application.Tests.Features.LaborLawRules.Command.UpdateLaborLawRule;

public class UpdateLaborLawRuleCommandValidatorTests
{
    private readonly UpdateLaborLawRuleCommandValidator _validator = new();

    private static readonly Guid ValidRuleId = Guid.NewGuid();
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const decimal ValidValue = 103_909_680m;

    private static UpdateLaborLawRuleCommand CreateValidCommand(
        Guid? ruleId = null,
        LaborLawRuleKey? key = LaborLawRuleKey.MinimumDailySalary,
        decimal? value = ValidValue,
        DateOnly? effectiveFrom = null) =>
        new(ruleId ?? ValidRuleId, key, value, effectiveFrom ?? ValidEffectiveFrom);

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRuleId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(ruleId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LaborLawRuleId);
    }

    [Fact]
    public void Validate_WithNullKey_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(key: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_WithNegativeValue_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(value: -1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_WithNullEffectiveFrom_ShouldHaveValidationError()
    {
        var command = new UpdateLaborLawRuleCommand(
            ValidRuleId,
            LaborLawRuleKey.MinimumDailySalary,
            ValidValue,
            null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EffectiveFrom);
    }
}
