namespace Application.Tests.Features.EmployeeSalaryProfiles.Queries.GetEmployeeSalaryProfiles;

public class GetEmployeeSalaryProfilesQueryHandlerTests
{
    private readonly IEmployeeSalaryProfileQuery _employeeSalaryProfileQuery;
    private readonly GetEmployeeSalaryProfilesQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private const string ValidSearch = "علی";
    private const EmployeeSalaryProfileStatus ValidStatus = EmployeeSalaryProfileStatus.Active;
    private static readonly PaginationDto ValidPagination = new(1, 10);

    public GetEmployeeSalaryProfilesQueryHandlerTests()
    {
        _employeeSalaryProfileQuery = Substitute.For<IEmployeeSalaryProfileQuery>();
        _handler = new GetEmployeeSalaryProfilesQueryHandler(_employeeSalaryProfileQuery);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPagedResult()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus);

        var results = new List<EmployeeSalaryProfileResult>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "علی رضایی",
                "EMP001",
                DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                71_661_840m,
                EmployeeSalaryProfileStatus.Active),
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "مینا احمدی",
                "EMP002",
                DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                60_000_000m,
                EmployeeSalaryProfileStatus.Expired)
        };
        var pagedResult = new PagedResult<EmployeeSalaryProfileResult>(results, 2, 1, 10);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
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
        firstItem.EmployeeSalaryProfileId.Should().Be(results[0].EmployeeSalaryProfileId);
        firstItem.EmployeeId.Should().Be(results[0].EmployeeId);
        firstItem.EmployeeName.Should().Be("علی رضایی");
        firstItem.PersonalCode.Should().Be("EMP001");
        firstItem.EffectiveFrom.Should().Be(results[0].EffectiveFrom);
        firstItem.BaseMonthlySalary.Should().Be(71_661_840m);
        firstItem.Status.Should().Be(EmployeeSalaryProfileStatus.Active);

        var secondItem = response.Items[1];
        secondItem.EmployeeName.Should().Be("مینا احمدی");
        secondItem.PersonalCode.Should().Be("EMP002");
        secondItem.EffectiveFrom.Should().Be(results[1].EffectiveFrom);
        secondItem.BaseMonthlySalary.Should().Be(60_000_000m);
        secondItem.Status.Should().Be(EmployeeSalaryProfileStatus.Expired);
    }

    [Fact]
    public async Task Handle_WithNoSalaryProfiles_ShouldReturnEmptyPagedResult()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus);

        var emptyPagedResult = new PagedResult<EmployeeSalaryProfileResult>([], 0, 1, 10);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
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
    public async Task Handle_ShouldCallGetEmployeeSalaryProfilesAsyncOnce()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus);

        var pagedResult = new PagedResult<EmployeeSalaryProfileResult>([], 0, 1, 10);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                ValidStatus,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfilesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            ValidStatus,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullFilters_ShouldCallQueryWithNullValues()
    {
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            null,
            null,
            null);

        var pagedResult = new PagedResult<EmployeeSalaryProfileResult>([], 0, 1, 10);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
                ValidUserId,
                ValidPagination,
                null,
                null,
                null,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfilesAsync(
            ValidUserId,
            ValidPagination,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSearch_ShouldPassSearchToQuery()
    {
        var search = "1234567890";
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            search,
            ValidStatus);

        var pagedResult = new PagedResult<EmployeeSalaryProfileResult>([], 0, 1, 10);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                search,
                ValidStatus,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfilesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            search,
            ValidStatus,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithStatus_ShouldPassStatusToQuery()
    {
        var status = EmployeeSalaryProfileStatus.Expired;
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            status);

        var pagedResult = new PagedResult<EmployeeSalaryProfileResult>([], 0, 1, 10);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
                ValidUserId,
                ValidPagination,
                ValidEmployeeId,
                ValidSearch,
                status,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfilesAsync(
            ValidUserId,
            ValidPagination,
            ValidEmployeeId,
            ValidSearch,
            status,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmployeeId_ShouldPassEmployeeIdToQuery()
    {
        var employeeId = Guid.NewGuid();
        var query = new GetEmployeeSalaryProfilesQuery(
            ValidUserId,
            ValidPagination,
            employeeId,
            ValidSearch,
            ValidStatus);

        var pagedResult = new PagedResult<EmployeeSalaryProfileResult>([], 0, 1, 10);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
                ValidUserId,
                ValidPagination,
                employeeId,
                ValidSearch,
                ValidStatus,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfilesAsync(
            ValidUserId,
            ValidPagination,
            employeeId,
            ValidSearch,
            ValidStatus,
            Arg.Any<CancellationToken>());
    }
}
