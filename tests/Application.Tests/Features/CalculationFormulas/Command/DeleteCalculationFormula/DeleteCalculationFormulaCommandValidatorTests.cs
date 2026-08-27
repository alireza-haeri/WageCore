namespace Application.Tests.Features.CalculationFormulas.Command.DeleteCalculationFormula;

public class DeleteCalculationFormulaCommandValidatorTests
{
    private readonly DeleteCalculationFormulaCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new DeleteCalculationFormulaCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyFormulaId_ShouldHaveValidationError()
    {
        var command = new DeleteCalculationFormulaCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CalculationFormulaId);
    }
}
