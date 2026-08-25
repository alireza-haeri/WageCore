namespace Application.Tests.Features.Departments.Queries.GetUserDepartmentsName;

public class GetUserDepartmentsNameQueryValidatorTests
{
    private readonly GetUserDepartmentsNameQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserDepartmentsNameQuery(ValidUserId);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetUserDepartmentsNameQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
