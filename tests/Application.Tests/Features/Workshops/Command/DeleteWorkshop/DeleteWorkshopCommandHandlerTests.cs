namespace Application.Tests.Features.Workshops.Command.DeleteWorkshop;

public class DeleteWorkshopCommandHandlerTests
{
    private readonly IWorkShopRepository _workShopRepository;
    private readonly DeleteWorkshopCommandHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    public DeleteWorkshopCommandHandlerTests()
    {
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _handler = new DeleteWorkshopCommandHandler(_workShopRepository);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldDeleteWorkshopAndReturnTrue()
    {
        var command = new DeleteWorkshopCommand(ValidUserId, ValidWorkshopId);

        _workShopRepository.DeleteAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRepositoryDeleteFails_ShouldReturnGeneralFailure()
    {
        var command = new DeleteWorkshopCommand(ValidUserId, ValidWorkshopId);

        _workShopRepository.DeleteAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteAsyncOnce()
    {
        var command = new DeleteWorkshopCommand(ValidUserId, ValidWorkshopId);

        _workShopRepository.DeleteAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1).DeleteAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToRepository()
    {
        var command = new DeleteWorkshopCommand(ValidUserId, ValidWorkshopId);

        _workShopRepository.DeleteAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1).DeleteAsync(
            ValidUserId,
            ValidWorkshopId,
            Arg.Any<CancellationToken>());
    }
}