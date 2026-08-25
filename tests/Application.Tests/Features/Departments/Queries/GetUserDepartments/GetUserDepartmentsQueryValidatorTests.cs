namespace Application.Tests.Features.Departments.Queries.GetUserDepartments;

public class GetUserDepartmentsQueryValidatorTests
{
    private readonly GetUserDepartmentsQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private const string ValidSearchName = "بخش";
    private static readonly PaginationDto ValidPagination = new(1, 10);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidWorkshopId);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidQueryAndNullSearchName_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            null,
            ValidWorkshopId);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidQueryAndNullWorkshopId_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            null);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidQueryAndNullSearchNameAndNullWorkshopId_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            null,
            null);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSearchNameExactly100Characters_ShouldNotHaveErrors()
    {
        var searchName = new string('a', 100);
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            searchName,
            ValidWorkshopId);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetUserDepartmentsQuery(
            Guid.Empty,
            ValidPagination,
            ValidSearchName,
            ValidWorkshopId);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyWorkshopId_ShouldHaveValidationError()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            Guid.Empty);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }

    [Fact]
    public void Validate_WithSearchNameMoreThan100Characters_ShouldHaveValidationError()
    {
        var searchName = new string('a', 101);
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            searchName,
            ValidWorkshopId);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.SearchName);
    }

    [Fact]
    public void Validate_WithInvalidPagination_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(0, 10);
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            invalidPagination,
            ValidSearchName,
            ValidWorkshopId);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageNumber);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageSize_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(1, 101);
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            invalidPagination,
            ValidSearchName,
            ValidWorkshopId);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }
}
