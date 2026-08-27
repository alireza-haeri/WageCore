namespace Application.Tests.Features.LaborLawRules.Queries.GetLaborLawRules;

public class GetLaborLawRulesQueryValidatorTests
{
    private readonly GetLaborLawRulesQueryValidator _validator = new();
    private static readonly PaginationDto ValidPagination = new(1, 10);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetLaborLawRulesQuery(ValidPagination, LaborLawRuleKey.MinimumMonthlySalary);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullKey_ShouldNotHaveAnyErrors()
    {
        var query = new GetLaborLawRulesQuery(ValidPagination);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidKey_ShouldHaveValidationError()
    {
        var query = new GetLaborLawRulesQuery(ValidPagination, (LaborLawRuleKey)99);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_WithInvalidPagination_ShouldHaveValidationError()
    {
        var query = new GetLaborLawRulesQuery(new PaginationDto(0, 10));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageNumber);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageSize_ShouldHaveValidationError()
    {
        var query = new GetLaborLawRulesQuery(new PaginationDto(1, 101));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }
}
