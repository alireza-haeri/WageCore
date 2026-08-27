namespace Application.Tests.Features.LaborLawRules.Command.DeleteLaborLawRule;

public class DeleteLaborLawRuleCommandValidatorTests
{
    private readonly DeleteLaborLawRuleCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new DeleteLaborLawRuleCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRuleId_ShouldHaveValidationError()
    {
        var command = new DeleteLaborLawRuleCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LaborLawRuleId);
    }
}
