namespace Application.Tests.Features.CalculationFormulas.Queries.GetCalculationFormulas;

public class GetCalculationFormulasQueryValidatorTests
{
    private readonly GetCalculationFormulasQueryValidator _validator = new();
    private static readonly PaginationDto ValidPagination = new(1, 10);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetCalculationFormulasQuery(ValidPagination, FormulaKey.OvertimePay);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullKey_ShouldNotHaveAnyErrors()
    {
        var query = new GetCalculationFormulasQuery(ValidPagination);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidKey_ShouldHaveValidationError()
    {
        var query = new GetCalculationFormulasQuery(ValidPagination, (FormulaKey)99);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_WithInvalidPagination_ShouldHaveValidationError()
    {
        var query = new GetCalculationFormulasQuery(new PaginationDto(0, 10));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageNumber);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageSize_ShouldHaveValidationError()
    {
        var query = new GetCalculationFormulasQuery(new PaginationDto(1, 101));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }
}
