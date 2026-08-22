namespace Application.Tests.Features.Workshops.Queries.GetUserWorkshopsName;

public class GetUserWorkshopsNameQueryHandlerTests
{
    private readonly IWorkshopQuery _workshopQuery;
    private readonly GetUserWorkshopsNameQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();

    public GetUserWorkshopsNameQueryHandlerTests()
    {
        _workshopQuery = Substitute.For<IWorkshopQuery>();
        _handler = new GetUserWorkshopsNameQueryHandler(_workshopQuery);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnListOfWorkshopNames()
    {
        var query = new GetUserWorkshopsNameQuery(ValidUserId);

        var workshopNames = new List<UserWorkshopNameResult>
        {
            new(Guid.NewGuid(), "کارگاه اول"),
            new(Guid.NewGuid(), "کارگاه دوم"),
            new(Guid.NewGuid(), "کارگاه سوم")
        };

        _workshopQuery.GetUserWorkshopsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(workshopNames);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.WorkshopNames.Should().HaveCount(3);
        response.WorkshopNames[0].WorkshopId.Should().Be(workshopNames[0].WorkshopId);
        response.WorkshopNames[0].DisplayName.Should().Be("کارگاه اول");
        response.WorkshopNames[1].WorkshopId.Should().Be(workshopNames[1].WorkshopId);
        response.WorkshopNames[1].DisplayName.Should().Be("کارگاه دوم");
        response.WorkshopNames[2].WorkshopId.Should().Be(workshopNames[2].WorkshopId);
        response.WorkshopNames[2].DisplayName.Should().Be("کارگاه سوم");
    }

    [Fact]
    public async Task Handle_WithNoWorkshops_ShouldReturnEmptyList()
    {
        var query = new GetUserWorkshopsNameQuery(ValidUserId);

        _workshopQuery.GetUserWorkshopsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(new List<UserWorkshopNameResult>());

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.WorkshopNames.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallGetUserWorkshopsNameAsyncOnce()
    {
        var query = new GetUserWorkshopsNameQuery(ValidUserId);

        _workshopQuery.GetUserWorkshopsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(new List<UserWorkshopNameResult>());

        await _handler.Handle(query, CancellationToken.None);

        await _workshopQuery.Received(1).GetUserWorkshopsNameAsync(ValidUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserIdToRepository()
    {
        var query = new GetUserWorkshopsNameQuery(ValidUserId);

        _workshopQuery.GetUserWorkshopsNameAsync(ValidUserId, Arg.Any<CancellationToken>())
            .Returns(new List<UserWorkshopNameResult>());

        await _handler.Handle(query, CancellationToken.None);

        await _workshopQuery.Received(1).GetUserWorkshopsNameAsync(
            ValidUserId,
            Arg.Any<CancellationToken>());
    }
}