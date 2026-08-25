namespace Application.Tests.Features.Departments.Queries.GetDepartmentForEdit;

public class GetDepartmentForEditQueryHandlerTests
{
    private readonly IDepartmentQuery _departmentQuery;
    private readonly GetDepartmentForEditQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    public GetDepartmentForEditQueryHandlerTests()
    {
        _departmentQuery = Substitute.For<IDepartmentQuery>();
        _handler = new GetDepartmentForEditQueryHandler(_departmentQuery);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnDepartmentDetails()
    {
        var query = new GetDepartmentForEditQuery(ValidUserId, ValidDepartmentId);

        var department = new UserDepartmentByIdResult("دپارتمان تولید", ValidWorkshopId);

        _departmentQuery.GetUserDepartmentByIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(department);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Name.Should().Be("دپارتمان تولید");
        response.WorkshopId.Should().Be(ValidWorkshopId);
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldReturnNotFoundFailure()
    {
        var query = new GetDepartmentForEditQuery(ValidUserId, ValidDepartmentId);

        _departmentQuery.GetUserDepartmentByIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((UserDepartmentByIdResult?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeFailure();
    }

    [Fact]
    public async Task Handle_ShouldCallGetUserDepartmentByIdAsyncOnce()
    {
        var query = new GetDepartmentForEditQuery(ValidUserId, ValidDepartmentId);

        _departmentQuery.GetUserDepartmentByIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((UserDepartmentByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentByIdAsync(
            ValidUserId,
            ValidDepartmentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToQuery()
    {
        var query = new GetDepartmentForEditQuery(ValidUserId, ValidDepartmentId);

        _departmentQuery.GetUserDepartmentByIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((UserDepartmentByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _departmentQuery.Received(1).GetUserDepartmentByIdAsync(
            ValidUserId,
            ValidDepartmentId,
            Arg.Any<CancellationToken>());
    }
}
