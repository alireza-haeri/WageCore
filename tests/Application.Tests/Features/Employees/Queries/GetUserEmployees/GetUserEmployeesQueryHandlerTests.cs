namespace Application.Tests.Features.Employees.Queries.GetUserEmployees;

public class GetUserEmployeesQueryHandlerTests
{
    private readonly IEmployeeQuery _employeeQuery;
    private readonly GetUserEmployeesQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private const string ValidSearch = "کارمند";
    private const EmployeeStatus ValidStatus = EmployeeStatus.Employed;
    private static readonly PaginationDto ValidPagination = new(1, 10);

    public GetUserEmployeesQueryHandlerTests()
    {
        _employeeQuery = Substitute.For<IEmployeeQuery>();
        _handler = new GetUserEmployeesQueryHandler(_employeeQuery);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPagedResult()
    {
        var query = new GetUserEmployeesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var results = new List<UserEmployeeResult>
        {
            new(
                Guid.NewGuid(),
                "EMP001",
                "علی رضایی",
                "کارگاه اول",
                "بخش تولید",
                "1234567890",
                DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                "حسابدار",
                EmployeeStatus.Employed,
                Region.Normal,
                3,
                45,
                5),
            new(
                Guid.NewGuid(),
                "EMP002",
                "مینا احمدی",
                "کارگاه دوم",
                "بخش اداری",
                "0987654321",
                DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                null,
                EmployeeStatus.Unemployed,
                Region.Normal)
        };
        var pagedResult = new PagedResult<UserEmployeeResult>(results, 2, 1, 10);

        _employeeQuery.GetUserEmployeesAsync(
                ValidUserId,
                ValidPagination,
                ValidSearch,
                ValidWorkshopId,
                ValidDepartmentId,
                ValidStatus,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Items.Should().HaveCount(2);
        response.TotalCount.Should().Be(2);
        response.PageNumber.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(1);

        var firstItem = response.Items[0];
        firstItem.Id.Should().Be(results[0].EmployeeId);
        firstItem.PersonalCode.Should().Be("EMP001");
        firstItem.FullName.Should().Be("علی رضایی");
        firstItem.WorkshopName.Should().Be("کارگاه اول");
        firstItem.DepartmentName.Should().Be("بخش تولید");
        firstItem.NationalCode.Should().Be("1234567890");
        firstItem.HireDate.Should().Be(results[0].HireDate);
        firstItem.JobTitle.Should().Be("حسابدار");
        firstItem.Status.Should().Be(EmployeeStatus.Employed);
        firstItem.LeaveUsedInCurrentYear.Should().Be(3);
        firstItem.NetWorkedDaysBeforeCurrentMonth.Should().Be(45);
        firstItem.CarriedOverLeaveFromPreviousYear.Should().Be(5);

        var secondItem = response.Items[1];
        secondItem.Id.Should().Be(results[1].EmployeeId);
        secondItem.PersonalCode.Should().Be("EMP002");
        secondItem.FullName.Should().Be("مینا احمدی");
        secondItem.WorkshopName.Should().Be("کارگاه دوم");
        secondItem.DepartmentName.Should().Be("بخش اداری");
        secondItem.NationalCode.Should().Be("0987654321");
        secondItem.HireDate.Should().Be(results[1].HireDate);
        secondItem.JobTitle.Should().BeNull();
        secondItem.Status.Should().Be(EmployeeStatus.Unemployed);
    }

    [Fact]
    public async Task Handle_WithNoEmployees_ShouldReturnEmptyPagedResult()
    {
        var query = new GetUserEmployeesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var emptyPagedResult = new PagedResult<UserEmployeeResult>([], 0, 1, 10);

        _employeeQuery.GetUserEmployeesAsync(
                ValidUserId,
                ValidPagination,
                ValidSearch,
                ValidWorkshopId,
                ValidDepartmentId,
                ValidStatus,
                Arg.Any<CancellationToken>())
            .Returns(emptyPagedResult);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.PageNumber.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCallGetUserEmployeesAsyncOnce()
    {
        var query = new GetUserEmployeesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var pagedResult = new PagedResult<UserEmployeeResult>([], 0, 1, 10);

        _employeeQuery.GetUserEmployeesAsync(
                ValidUserId,
                ValidPagination,
                ValidSearch,
                ValidWorkshopId,
                ValidDepartmentId,
                ValidStatus,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeQuery.Received(1).GetUserEmployeesAsync(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullFilters_ShouldCallRepositoryWithNullValues()
    {
        var query = new GetUserEmployeesQuery(
            ValidUserId,
            ValidPagination,
            null,
            null,
            null,
            null);

        var pagedResult = new PagedResult<UserEmployeeResult>([], 0, 1, 10);

        _employeeQuery.GetUserEmployeesAsync(
                ValidUserId,
                ValidPagination,
                null,
                null,
                null,
                null,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeQuery.Received(1).GetUserEmployeesAsync(
            ValidUserId,
            ValidPagination,
            null,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSearch_ShouldPassSearchToRepository()
    {
        var search = "1234567890";
        var query = new GetUserEmployeesQuery(
            ValidUserId,
            ValidPagination,
            search,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus);

        var pagedResult = new PagedResult<UserEmployeeResult>([], 0, 1, 10);

        _employeeQuery.GetUserEmployeesAsync(
                ValidUserId,
                ValidPagination,
                search,
                ValidWorkshopId,
                ValidDepartmentId,
                ValidStatus,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeQuery.Received(1).GetUserEmployeesAsync(
            ValidUserId,
            ValidPagination,
            search,
            ValidWorkshopId,
            ValidDepartmentId,
            ValidStatus,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithStatus_ShouldPassStatusToRepository()
    {
        var status = EmployeeStatus.Unemployed;
        var query = new GetUserEmployeesQuery(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            status);

        var pagedResult = new PagedResult<UserEmployeeResult>([], 0, 1, 10);

        _employeeQuery.GetUserEmployeesAsync(
                ValidUserId,
                ValidPagination,
                ValidSearch,
                ValidWorkshopId,
                ValidDepartmentId,
                status,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeQuery.Received(1).GetUserEmployeesAsync(
            ValidUserId,
            ValidPagination,
            ValidSearch,
            ValidWorkshopId,
            ValidDepartmentId,
            status,
            Arg.Any<CancellationToken>());
    }
}
