namespace Application.Tests.Features.Departments.Command.CreateDepartment;

public class CreateDepartmentCommandHandlerTests
{
    private readonly IWorkShopRepository _workShopRepository;
    private readonly IDepartmentQuery _departmentQuery;
    private readonly CreateDepartmentCommandHandler _handler;
    private readonly WorkshopBuilder _workshopBuilder;

    private const string ValidName = "دپارتمان نمونه";
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    public CreateDepartmentCommandHandlerTests()
    {
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _departmentQuery = Substitute.For<IDepartmentQuery>();
        _workshopBuilder = new WorkshopBuilder();

        _handler = new CreateDepartmentCommandHandler(_workShopRepository, _departmentQuery);
    }

    private Workshop CreateValidWorkshop()
    {
        return _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private void SetupNoDuplicates()
    {
        _departmentQuery.IsExistDepartmentName(ValidUserId, Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateDepartmentAndReturnId()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.DepartmentId.Should().NotBeEmpty();
        workshop.Departments.Should().Contain(d => d.Id == response.DepartmentId && d.Name == ValidName);
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotFoundFailure()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDepartmentNameIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _departmentQuery.IsExistDepartmentName(ValidUserId, ValidName, null, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnGeneralFailure()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, "");
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsyncOnce()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1).GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncOnce()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1).UpdateAsync(workshop, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldNotCallUpdateAsync()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameIsDuplicate_ShouldNotCallUpdateAsync()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _departmentQuery.IsExistDepartmentName(ValidUserId, ValidName, null, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainFails_ShouldNotCallUpdateAsync()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, "");
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldNotCallIsExistDepartmentName()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _departmentQuery.DidNotReceive()
            .IsExistDepartmentName(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
