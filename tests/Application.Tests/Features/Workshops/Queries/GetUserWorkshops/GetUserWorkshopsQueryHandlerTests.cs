namespace Application.Tests.Features.Workshops.Queries.GetUserWorkshops;

public class GetUserWorkshopsQueryHandlerTests
{
    private readonly IWorkshopQuery _workshopQuery;
    private readonly GetUserWorkshopsQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidSearchName = "کارگاه";
    private const WorkshopRegion ValidRegion = WorkshopRegion.Normal;
    private static readonly PaginationDto ValidPagination = new(1, 10);

    public GetUserWorkshopsQueryHandlerTests()
    {
        _workshopQuery = Substitute.For<IWorkshopQuery>();
        _handler = new GetUserWorkshopsQueryHandler(_workshopQuery);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidRegion);

        var results = new List<UserWorkshopResult>
        {
            new(Guid.NewGuid(), "کارگاه اول", "آدرس ۱", WorkshopRegion.Normal, DateOnly.FromDateTime(DateTime.Now), 5, 3),
            new(Guid.NewGuid(), "کارگاه دوم", "آدرس ۲", WorkshopRegion.LessDeveloped, DateOnly.FromDateTime(DateTime.Now), 10, 4)
        };
        var pagedResult = new PagedResult<UserWorkshopResult>(results, 2, 1, 10);

        _workshopQuery.GetUserWorkshopsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                ValidRegion,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var response = result.ShouldBeSuccess();
        response.Items.Should().HaveCount(2);
        response.TotalCount.Should().Be(2);
        response.PageNumber.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(1);

        var firstItem = response.Items[0];
        firstItem.Id.Should().Be(results[0].WorkshopId);
        firstItem.Name.Should().Be("کارگاه اول");
        firstItem.Address.Should().Be("آدرس ۱");
        firstItem.Region.Should().Be(WorkshopRegion.Normal);
        firstItem.EmployeesCount.Should().Be(5);
        firstItem.DepartmentsCount.Should().Be(3);

        var secondItem = response.Items[1];
        secondItem.Id.Should().Be(results[1].WorkshopId);
        secondItem.Name.Should().Be("کارگاه دوم");
        secondItem.Address.Should().Be("آدرس ۲");
        secondItem.Region.Should().Be(WorkshopRegion.LessDeveloped);
        secondItem.EmployeesCount.Should().Be(10);
        secondItem.DepartmentsCount.Should().Be(4);
    }

    [Fact]
    public async Task Handle_WithNoWorkshops_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidRegion);

        var emptyPagedResult = new PagedResult<UserWorkshopResult>([], 0, 1, 10);

        _workshopQuery.GetUserWorkshopsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                ValidRegion,
                Arg.Any<CancellationToken>())
            .Returns(emptyPagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var response = result.ShouldBeSuccess();
        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.PageNumber.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCallGetUserWorkshopsAsyncOnce()
    {
        // Arrange
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidRegion);

        var pagedResult = new PagedResult<UserWorkshopResult>([], 0, 1, 10);

        _workshopQuery.GetUserWorkshopsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                ValidRegion,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _workshopQuery.Received(1).GetUserWorkshopsAsync(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            ValidRegion,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullSearchNameAndNullRegion_ShouldCallRepositoryWithNullValues()
    {
        // Arrange
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            null,
            null);

        var pagedResult = new PagedResult<UserWorkshopResult>([], 0, 1, 10);

        _workshopQuery.GetUserWorkshopsAsync(
                ValidUserId,
                ValidPagination,
                null,
                null,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _workshopQuery.Received(1).GetUserWorkshopsAsync(
            ValidUserId,
            ValidPagination,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSearchName_ShouldPassSearchNameToRepository()
    {
        // Arrange
        var searchName = "نساجی";
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            searchName,
            ValidRegion);

        var pagedResult = new PagedResult<UserWorkshopResult>([], 0, 1, 10);

        _workshopQuery.GetUserWorkshopsAsync(
                ValidUserId,
                ValidPagination,
                searchName,
                ValidRegion,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _workshopQuery.Received(1).GetUserWorkshopsAsync(
            ValidUserId,
            ValidPagination,
            searchName,
            ValidRegion,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithRegion_ShouldPassRegionToRepository()
    {
        // Arrange
        var region = WorkshopRegion.LessDeveloped;
        var query = new GetUserWorkshopsQuery(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            region);

        var pagedResult = new PagedResult<UserWorkshopResult>([], 0, 1, 10);

        _workshopQuery.GetUserWorkshopsAsync(
                ValidUserId,
                ValidPagination,
                ValidSearchName,
                region,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _workshopQuery.Received(1).GetUserWorkshopsAsync(
            ValidUserId,
            ValidPagination,
            ValidSearchName,
            region,
            Arg.Any<CancellationToken>());
    }
}