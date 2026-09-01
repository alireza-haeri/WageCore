namespace Application.Tests.Features.Workshops.Queries.GetUserWorkshops;

public class GetUserWorkshopsQueryValidatorTests
{
    private readonly GetUserWorkshopsQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidSearchName = "کارگاه";
    private static readonly PaginationDto ValidPagination = new(1, 10);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidQueryAndNullSearchName_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            null);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSearchNameExactly200Characters_ShouldNotHaveErrors()
    {
        var searchName = new string('a', 200);
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            searchName);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetUserWorkshopsQuery(
            Guid.Empty,
            ValidPagination,
            ValidSearchName);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithSearchNameMoreThan200Characters_ShouldHaveValidationError()
    {
        var searchName = new string('a', 201);
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            searchName);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.SearchName);
    }

    [Fact]
    public void Validate_WithInvalidPagination_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(0, 10);
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            invalidPagination,
            ValidSearchName);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageNumber);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageSize_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(1, 101);
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            invalidPagination,
            ValidSearchName);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }
}
