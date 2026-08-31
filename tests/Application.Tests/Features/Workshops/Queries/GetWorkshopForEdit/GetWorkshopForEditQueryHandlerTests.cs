namespace Application.Tests.Features.Workshops.Queries.GetWorkshopForEdit;

public class GetWorkshopForEditQueryHandlerTests
{
    private readonly IWorkshopQuery _workshopQuery;
    private readonly GetWorkshopForEditQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    public GetWorkshopForEditQueryHandlerTests()
    {
        _workshopQuery = Substitute.For<IWorkshopQuery>();
        _handler = new GetWorkshopForEditQueryHandler(_workshopQuery);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnWorkshopDetails()
    {
        var query = new GetWorkshopForEditQuery(ValidUserId, ValidWorkshopId);

        var workshop = new UserWorkshopByIdResult(
            "کارگاه آریا",
            "تهران، خیابان اصلی، پلاک ۱۰",
            DateOnly.FromDateTime(DateTime.Today),
            "1234567890",
            "0987654321");

        _workshopQuery.GetUserWorkshopByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Name.Should().Be("کارگاه آریا");
        response.Address.Should().Be("تهران، خیابان اصلی، پلاک ۱۰");
        response.RegistrationDate.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        response.NationalId.Should().Be("1234567890");
        response.PostalCode.Should().Be("0987654321");
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotFoundFailure()
    {
        var query = new GetWorkshopForEditQuery(ValidUserId, ValidWorkshopId);

        _workshopQuery.GetUserWorkshopByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((UserWorkshopByIdResult?)null);

        var result = await _handler.Handle(query, CancellationToken.None);
        result.ShouldBeFailure();
    }

    [Fact]
    public async Task Handle_ShouldCallGetUserWorkshopByIdAsyncOnce()
    {
        var query = new GetWorkshopForEditQuery(ValidUserId, ValidWorkshopId);

        _workshopQuery.GetUserWorkshopByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((UserWorkshopByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _workshopQuery.Received(1).GetUserWorkshopByIdAsync(
            ValidUserId,
            ValidWorkshopId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToQuery()
    {
        var query = new GetWorkshopForEditQuery(ValidUserId, ValidWorkshopId);

        _workshopQuery.GetUserWorkshopByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((UserWorkshopByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _workshopQuery.Received(1).GetUserWorkshopByIdAsync(
            ValidUserId,
            ValidWorkshopId,
            Arg.Any<CancellationToken>());
    }
}
