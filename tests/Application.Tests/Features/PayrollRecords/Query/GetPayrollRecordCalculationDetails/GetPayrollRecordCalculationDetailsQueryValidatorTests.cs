using FluentValidation.TestHelper;

namespace Application.Tests.Features.PayrollRecords.Query.GetPayrollRecordCalculationDetails;

public class GetPayrollRecordCalculationDetailsQueryValidatorTests
{
    private readonly GetPayrollRecordCalculationDetailsQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetPayrollRecordCalculationDetailsQuery(ValidUserId, ValidPayrollRecordId);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetPayrollRecordCalculationDetailsQuery(Guid.Empty, ValidPayrollRecordId);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyPayrollRecordId_ShouldHaveValidationError()
    {
        var query = new GetPayrollRecordCalculationDetailsQuery(ValidUserId, Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecordId);
    }
}
