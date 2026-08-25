namespace Application.Tests.Features.Departments.Queries.GetDepartmentForEdit;

public class GetDepartmentForEditQueryValidatorTests
{
    private readonly GetDepartmentForEditQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetDepartmentForEditQuery(ValidUserId, ValidDepartmentId);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetDepartmentForEditQuery(Guid.Empty, ValidDepartmentId);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var query = new GetDepartmentForEditQuery(ValidUserId, Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }
}
