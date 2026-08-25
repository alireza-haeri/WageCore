namespace Application.Tests.Features.Departments.Command.UpdateDepartment;

public class UpdateDepartmentCommandHandlerTests
{
    private readonly IWorkShopRepository _workShopRepository;
    private readonly IDepartmentQuery _departmentQuery;
    private readonly UpdateDepartmentCommandHandler _handler;
    private readonly WorkshopBuilder _workshopBuilder;

    private const string ValidName = "دپارتمان نمونه";
    private const string UpdatedName = "دپارتمان جدید";
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();

    public UpdateDepartmentCommandHandlerTests()
    {
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _departmentQuery = Substitute.For<IDepartmentQuery>();
        _workshopBuilder = new WorkshopBuilder();

        _handler = new UpdateDepartmentCommandHandler(_workShopRepository, _departmentQuery);
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

    private UpdateDepartmentCommand CreateValidCommand(string? name = null)
    {
        return new UpdateDepartmentCommand(ValidUserId, ValidDepartmentId, name ?? UpdatedName);
    }

    private void SetupNoDuplicates()
    {
        _departmentQuery.IsExistDepartmentName(ValidUserId, Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateDepartmentAndReturnTrue()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        workshop.Departments.First(d => d.Id == ValidDepartmentId).Name.Should().Be(UpdatedName);
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDepartmentNameIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _departmentQuery.IsExistDepartmentName(ValidUserId, UpdatedName, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(name: "");
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallGetByDepartmentIdAsyncOnce()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1)
            .GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncOnce()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1).UpdateAsync(workshop, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldNotCallUpdateAsync()
    {
        var command = CreateValidCommand();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldNotCallUpdateAsync()
    {
        var command = CreateValidCommand(name: "");
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameIsDuplicate_ShouldNotCallUpdateAsync()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshopWithDepartment();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _departmentQuery.IsExistDepartmentName(ValidUserId, UpdatedName, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldNotCallIsExistDepartmentName()
    {
        var command = CreateValidCommand();

        _workShopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _departmentQuery.DidNotReceive()
            .IsExistDepartmentName(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
