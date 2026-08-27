namespace Application.Tests.Features.CalculationFormulas.Queries.GetCalculationFormulaForEdit;

public class GetCalculationFormulaForEditQueryValidatorTests
{
    private readonly GetCalculationFormulaForEditQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetCalculationFormulaForEditQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyFormulaId_ShouldHaveValidationError()
    {
        var query = new GetCalculationFormulaForEditQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.CalculationFormulaId);
    }
}
