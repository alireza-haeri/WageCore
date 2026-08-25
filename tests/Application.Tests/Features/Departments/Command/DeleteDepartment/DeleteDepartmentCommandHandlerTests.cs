namespace Application.Tests.Features.Departments.Command.DeleteDepartment;

public class DeleteDepartmentCommandHandlerTests
{
    private readonly IWorkShopRepository _workShopRepository;
    private readonly DeleteDepartmentCommandHandler _handler;
    private readonly WorkshopBuilder _workshopBuilder;

    private const string ValidName = "دپارتمان نمونه";
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();

    public DeleteDepartmentCommandHandlerTests()
    {
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _workshopBuilder = new WorkshopBuilder();
        _handler = new DeleteDepartmentCommandHandler(_workShopRepository);
    }

    private Workshop CreateValidWorkshopWithDepartment()
    {
        var workshop = _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .CreateResult()
            .ShouldBeSuccess();

        workshop.CreateDepartment(ValidDepartmentId, ValidName).ShouldBeSuccess();
        return workshop;
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldDeleteDepartmentAndReturnTrue()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, ValidDepartmentId);
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        workshop.Departments.Should().NotContain(d => d.Id == ValidDepartmentId);
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldReturnGeneralFailure()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, ValidDepartmentId);

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, ValidDepartmentId);
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallGetByDepartmentIdAsyncOnce()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, ValidDepartmentId);
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1)
            .GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncOnce()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, ValidDepartmentId);
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1).UpdateAsync(workshop, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldNotCallUpdateAsync()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, ValidDepartmentId);

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }
}
