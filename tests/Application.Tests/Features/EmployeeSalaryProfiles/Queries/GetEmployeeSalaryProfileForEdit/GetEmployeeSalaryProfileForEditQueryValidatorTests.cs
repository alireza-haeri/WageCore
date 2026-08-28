namespace Application.Tests.Features.EmployeeSalaryProfiles.Queries.GetEmployeeSalaryProfileForEdit;

public class GetEmployeeSalaryProfileForEditQueryValidatorTests
{
    private readonly GetEmployeeSalaryProfileForEditQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetEmployeeSalaryProfileForEditQuery(ValidUserId, ValidSalaryProfileId);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetEmployeeSalaryProfileForEditQuery(Guid.Empty, ValidSalaryProfileId);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptySalaryProfileId_ShouldHaveValidationError()
    {
        var query = new GetEmployeeSalaryProfileForEditQuery(ValidUserId, Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeSalaryProfileId);
    }
}
