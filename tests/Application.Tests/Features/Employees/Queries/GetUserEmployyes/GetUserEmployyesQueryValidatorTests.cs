namespace Application.Tests.Features.Employees.Queries.GetUserEmployyes;

public class GetUserEmployyesQueryValidatorTests
{
    private readonly GetUserEmployyesQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private const string ValidSearch = "کارمند";
    private const EmployeeStatus ValidStatus = EmployeeStatus.Employed;
    private static readonly PaginationDto ValidPagination = new(1, 10);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullOptionalFilters_ShouldNotHaveAnyErrors()
    {
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            ValidPagination,
            null,
            null,
            null,
            null);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSearchExactly100Characters_ShouldNotHaveErrors()
    {
        var search = new string('a', 100);
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            ValidPagination,
            search,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetUserEmployyesQuery(
            Guid.Empty,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyWorkshopId_ShouldHaveValidationError()
    {
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            Guid.Empty,
            ValidDepartmentId,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }

    [Fact]
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            Guid.Empty,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            (EmployeeStatus)999);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithSearchMoreThan100Characters_ShouldHaveValidationError()
    {
        var search = new string('a', 101);
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            ValidPagination,
            search,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageNumber_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(0, 10);
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            invalidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageNumber);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageSize_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(1, 101);
        var query = new GetUserEmployyesQuery(
            ValidUserId,
            invalidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }
}
