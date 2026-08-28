namespace Application.Tests.Features.EmployeeSalaryProfiles.Queries.GetEmployeeSalaryProfiles;

public class GetEmployeeSalaryProfilesQueryValidatorTests
{
    private readonly GetEmployeeSalaryProfilesQueryValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private const string ValidSearch = "علی";
    private const EmployeeSalaryProfileStatus ValidStatus = EmployeeSalaryProfileStatus.Active;
    private static readonly PaginationDto ValidPagination = new(1, 10);

    private static GetEmployeeSalaryProfilesQuery CreateValidQuery(
        Guid? userId = null,
        PaginationDto? pagination = null,
        Guid? employeeId = null,
        string? search = null,
        EmployeeSalaryProfileStatus? status = null,
        Guid? workshopId = null,
        Guid? departmentId = null) =>
        new(
            userId ?? ValidUserId,
            pagination ?? ValidPagination,
            employeeId ?? ValidEmployeeId,
            search ?? ValidSearch,
            status ?? ValidStatus,
            workshopId ?? ValidWorkshopId,
            departmentId ?? ValidDepartmentId);

    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveAnyErrors()
    {
        var query = CreateValidQuery();

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(userId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(employeeId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithEmptyWorkshopId_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(workshopId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }

    [Fact]
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(departmentId: Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(status: (EmployeeSalaryProfileStatus)999);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithSearchMoreThan100Characters_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(search: new string('a', 101));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageNumber_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(pagination: new PaginationDto(0, 10));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageNumber);
    }

    [Fact]
    public void Validate_WithInvalidPaginationPageSize_ShouldHaveValidationError()
    {
        var query = CreateValidQuery(pagination: new PaginationDto(1, 101));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }
}
