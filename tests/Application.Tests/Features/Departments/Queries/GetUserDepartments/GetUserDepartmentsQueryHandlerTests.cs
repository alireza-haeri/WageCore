namespace Application.Tests.Features.Departments.Queries.GetUserDepartments;

public class GetUserDepartmentsQueryHandlerTests
{
    private readonly IDepartmentQuery _departmentQuery;
    private readonly GetUserDepartmentsQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private const string ValidSearchName = "بخش";
    private static readonly PaginationDto ValidPagination = new(1, 10);

    public GetUserDepartmentsQueryHandlerTests()
    {
        _departmentQuery = Substitute.For<IDepartmentQuery>();
        _handler = new GetUserDepartmentsQueryHandler(_departmentQuery);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPagedResult()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidWorkshopId);

        var workshopId1 = Guid.NewGuid();
        var workshopId2 = Guid.NewGuid();
        var results = new List<UserDepartmentResult>
        {
            new(Guid.NewGuid(), "بخش تولید", workshopId1, "کارگاه اول", 5),
            new(Guid.NewGuid(), "بخش انبار", workshopId2, "کارگاه دوم", 10)
        };
        var pagedResult = new PagedResult<UserDepartmentResult>(results, 2, 1, 10);

        _departmentQuery.GetUserDepartmentsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                ValidWorkshopId,
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
        firstItem.Id.Should().Be(results[0].DepartmentId);
        firstItem.Name.Should().Be("بخش تولید");
        firstItem.WorkshopId.Should().Be(workshopId1);
        firstItem.WorkshopName.Should().Be("کارگاه اول");
        firstItem.EmployeesCount.Should().Be(5);

        var secondItem = response.Items[1];
        secondItem.Id.Should().Be(results[1].DepartmentId);
        secondItem.Name.Should().Be("بخش انبار");
        secondItem.WorkshopId.Should().Be(workshopId2);
        secondItem.WorkshopName.Should().Be("کارگاه دوم");
        secondItem.EmployeesCount.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithNoDepartments_ShouldReturnEmptyPagedResult()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidWorkshopId);

        var emptyPagedResult = new PagedResult<UserDepartmentResult>([], 0, 1, 10);

        _departmentQuery.GetUserDepartmentsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                ValidWorkshopId,
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
    public async Task Handle_ShouldCallGetUserDepartmentsAsyncOnce()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidWorkshopId);

        var pagedResult = new PagedResult<UserDepartmentResult>([], 0, 1, 10);

        _departmentQuery.GetUserDepartmentsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                ValidWorkshopId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentsAsync(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidWorkshopId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullSearchNameAndNullWorkshopId_ShouldCallRepositoryWithNullValues()
    {
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            null,
            null);

        var pagedResult = new PagedResult<UserDepartmentResult>([], 0, 1, 10);

        _departmentQuery.GetUserDepartmentsAsync(
                ValidUserId,
                ValidPagination,
                null,
                null,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentsAsync(
            ValidUserId,
            ValidPagination,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSearchName_ShouldPassSearchNameToRepository()
    {
        var searchName = "تولید";
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            searchName,
            ValidWorkshopId);

        var pagedResult = new PagedResult<UserDepartmentResult>([], 0, 1, 10);

        _departmentQuery.GetUserDepartmentsAsync(
                ValidUserId,
                ValidPagination,
                searchName,
                ValidWorkshopId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentsAsync(
            ValidUserId,
            ValidPagination,
            searchName,
            ValidWorkshopId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWorkshopId_ShouldPassWorkshopIdToRepository()
    {
        var workshopId = Guid.NewGuid();
        var query = new GetUserDepartmentsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            workshopId);

        var pagedResult = new PagedResult<UserDepartmentResult>([], 0, 1, 10);

        _departmentQuery.GetUserDepartmentsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                workshopId,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentsAsync(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            workshopId,
            Arg.Any<CancellationToken>());
    }
}
