namespace Application.Tests.Features.SalaryDecrees.Queries.GetSalaryDecrees;

public class GetSalaryDecreesQueryHandlerTests
{
    private readonly ISalaryDecreeQuery _salaryDecreeQuery;
    private readonly GetSalaryDecreesQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private const string ValidSearch = "علی";
    private const SalaryDecreeStatus ValidStatus = SalaryDecreeStatus.Active;
    private static readonly PaginationDto ValidPagination = new(1, 10);

    public GetSalaryDecreesQueryHandlerTests()
    {
        _salaryDecreeQuery = Substitute.For<ISalaryDecreeQuery>();
        _handler = new GetSalaryDecreesQueryHandler(_salaryDecreeQuery);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPagedResult()
    {
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId);

        var results = new List<SalaryDecreeResult>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "علی رضایی",
                "EMP001",
                "کارگاه اول",
                "بخش تولید",
                DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                71_661_840m,
                SalaryDecreeStatus.Active),
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "مینا احمدی",
                "EMP002",
                "کارگاه دوم",
                "بخش اداری",
                DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                60_000_000m,
                SalaryDecreeStatus.Expired)
        };
        var pagedResult = new PagedResult<SalaryDecreeResult>(results, 2, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                ValidStatus,
                ValidWorkshopId,
                ValidDepartmentId,
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
        firstItem.SalaryDecreeId.Should().Be(results[0].SalaryDecreeId);
        firstItem.EmployeeId.Should().Be(results[0].EmployeeId);
        firstItem.EmployeeName.Should().Be("علی رضایی");
        firstItem.PersonalCode.Should().Be("EMP001");
        firstItem.WorkshopName.Should().Be("کارگاه اول");
        firstItem.DepartmentName.Should().Be("بخش تولید");
        firstItem.EffectiveFrom.Should().Be(results[0].EffectiveFrom);
        firstItem.BaseDailySalary.Should().Be(71_661_840m);
        firstItem.Status.Should().Be(SalaryDecreeStatus.Active);

        var secondItem = response.Items[1];
        secondItem.EmployeeName.Should().Be("مینا احمدی");
        secondItem.PersonalCode.Should().Be("EMP002");
        secondItem.WorkshopName.Should().Be("کارگاه دوم");
        secondItem.DepartmentName.Should().Be("بخش اداری");
        secondItem.EffectiveFrom.Should().Be(results[1].EffectiveFrom);
        secondItem.BaseDailySalary.Should().Be(60_000_000m);
        secondItem.Status.Should().Be(SalaryDecreeStatus.Expired);
    }

    [Fact]
    public async Task Handle_WithNoSalaryProfiles_ShouldReturnEmptyPagedResult()
    {
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId);

        var emptyPagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                ValidStatus,
                ValidWorkshopId,
                ValidDepartmentId,
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
    public async Task Handle_ShouldCallGetSalaryDecreesAsyncOnce()
    {
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId);

        var pagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                ValidStatus,
                ValidWorkshopId,
                ValidDepartmentId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullFilters_ShouldCallQueryWithNullValues()
    {
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            null,
            null,
            null,
            null,
            null);

        var pagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                null,
                null,
                null,
                null,
                null,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAsync(
            ValidUserId,
            ValidPagination,
            null,
            null,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSearch_ShouldPassSearchToQuery()
    {
        var search = "1234567890";
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            search,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId);

        var pagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                search,
                ValidStatus,
                ValidWorkshopId,
                ValidDepartmentId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            search,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithStatus_ShouldPassStatusToQuery()
    {
        var status = SalaryDecreeStatus.Expired;
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            status,
            ValidWorkshopId,
            ValidDepartmentId);

        var pagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                status,
                ValidWorkshopId,
                ValidDepartmentId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            status,
            ValidWorkshopId,
            ValidDepartmentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmployeeId_ShouldPassEmployeeIdToQuery()
    {
        var employeeId = Guid.NewGuid();
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            employeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId);

        var pagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                employeeId,
                ValidSearch,
                ValidStatus,
                ValidWorkshopId,
                ValidDepartmentId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAsync(
            ValidUserId,
            ValidPagination,
            employeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            ValidDepartmentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWorkshopId_ShouldPassWorkshopIdToQuery()
    {
        var workshopId = Guid.NewGuid();
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            workshopId,
            ValidDepartmentId);

        var pagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                ValidStatus,
                workshopId,
                ValidDepartmentId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            workshopId,
            ValidDepartmentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDepartmentId_ShouldPassDepartmentIdToQuery()
    {
        var departmentId = Guid.NewGuid();
        var query = new GetSalaryDecreesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            departmentId);

        var pagedResult = new PagedResult<SalaryDecreeResult>([], 0, 1, 10);

        _salaryDecreeQuery.GetSalaryDecreesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                ValidStatus,
                ValidWorkshopId,
                departmentId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            ValidWorkshopId,
            departmentId,
            Arg.Any<CancellationToken>());
    }
}
