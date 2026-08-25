namespace Application.Tests.Features.Departments.Queries.GetUserDepartmentsName;

public class GetUserDepartmentsNameQueryHandlerTests
{
    private readonly IDepartmentQuery _departmentQuery;
    private readonly GetUserDepartmentsNameQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();

    public GetUserDepartmentsNameQueryHandlerTests()
    {
        _departmentQuery = Substitute.For<IDepartmentQuery>();
        _handler = new GetUserDepartmentsNameQueryHandler(_departmentQuery);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnListOfDepartmentNames()
    {
        var query = new GetUserDepartmentsNameQuery(ValidUserId);

        var departmentNames = new List<UserDepartmentNameResult>
        {
            new(Guid.NewGuid(), "دپارتمان اول"),
            new(Guid.NewGuid(), "دپارتمان دوم"),
            new(Guid.NewGuid(), "دپارتمان سوم")
        };

        _departmentQuery.GetUserDepartmentsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(departmentNames);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.DepartmentNames.Should().HaveCount(3);
        response.DepartmentNames[0].DepartmentId.Should().Be(departmentNames[0].DepartmentId);
        response.DepartmentNames[0].DisplayName.Should().Be("دپارتمان اول");
        response.DepartmentNames[1].DepartmentId.Should().Be(departmentNames[1].DepartmentId);
        response.DepartmentNames[1].DisplayName.Should().Be("دپارتمان دوم");
        response.DepartmentNames[2].DepartmentId.Should().Be(departmentNames[2].DepartmentId);
        response.DepartmentNames[2].DisplayName.Should().Be("دپارتمان سوم");
    }

    [Fact]
    public async Task Handle_WithNoDepartments_ShouldReturnEmptyList()
    {
        var query = new GetUserDepartmentsNameQuery(ValidUserId);

        _departmentQuery.GetUserDepartmentsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDepartmentNameResult>());

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.DepartmentNames.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallGetUserDepartmentsNameAsyncOnce()
    {
        var query = new GetUserDepartmentsNameQuery(ValidUserId);

        _departmentQuery.GetUserDepartmentsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDepartmentNameResult>());

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentsNameAsync(ValidUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserIdToRepository()
    {
        var query = new GetUserDepartmentsNameQuery(ValidUserId);

        _departmentQuery.GetUserDepartmentsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(new List<UserDepartmentNameResult>());

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentsNameAsync(
            ValidUserId,
            Arg.Any<CancellationToken>());
    }
}
