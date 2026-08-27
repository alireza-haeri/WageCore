namespace Application.Tests.Features.LaborLawRules.Queries.GetLaborLawRuleForEdit;

public class GetLaborLawRuleForEditQueryValidatorTests
{
    private readonly GetLaborLawRuleForEditQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetLaborLawRuleForEditQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRuleId_ShouldHaveValidationError()
    {
        var query = new GetLaborLawRuleForEditQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.LaborLawRuleId);
    }
}
