namespace Application.Tests.Features.EmployeeSalaryProfiles.Queries.GetEmployeeSalaryProfiles;

public class GetEmployeeSalaryProfilesQueryValidatorTests
{
    private readonly GetEmployeeSalaryProfilesQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private const string ValidSearch = "علی";
    private const EmployeeSalaryProfileStatus ValidStatus = EmployeeSalaryProfileStatus.Active;
    private static readonly PaginationDto ValidPagination = new(1, 10);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            Guid.Empty,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            Guid.Empty,
            ValidSearch,
            ValidStatus);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            (EmployeeSalaryProfileStatus)999);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithSearchMoreThan100Characters_ShouldHaveValidationError()
    {
        var search = new string('a', 101);
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            search,
            ValidStatus);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageNumber_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(0, 10);
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            invalidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageNumber);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageSize_ShouldHaveValidationError()
    {
        var invalidPagination = new PaginationDto(1, 101);
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            invalidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }
}
