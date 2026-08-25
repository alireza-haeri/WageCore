namespace Application.Tests.Features.Workshops.Queries.GetUserWorkshopsName;

public class GetUserWorkshopsNameQueryValidatorTests
{
    private readonly GetUserWorkshopsNameQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserWorkshopsNameQuery(ValidUserId);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetUserWorkshopsNameQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}