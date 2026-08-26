namespace Application.Tests.Features.Employees.Queries.GetUserEmployeeForEdit;

public class GetUserEmployeeForEditQueryValidatorTests
{
    private readonly GetUserEmployeeForEditQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserEmployeeForEditQuery(ValidUserId, ValidEmployeeId);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetUserEmployeeForEditQuery(Guid.Empty, ValidEmployeeId);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var query = new GetUserEmployeeForEditQuery(ValidUserId, Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }
}
