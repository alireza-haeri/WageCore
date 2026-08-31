namespace Application.Tests.Features.Workshops.Command.UpdateWorkshop;

public class UpdateWorkshopCommandHandlerTests
{
    private readonly IWorkShopRepository _workShopRepository;
    private readonly IWorkshopQuery _workshopQuery;
    private readonly UpdateWorkshopCommandHandler _handler;
    private readonly WorkshopBuilder _workshopBuilder;

    private const string ValidName = "کارگاه نمونه";
    private const string ValidAddress = "تهران، خیابان نمونه، پلاک ۱۲۳";
    private static readonly DateOnly ValidRegistrationDate = DateOnly.FromDateTime(DateTime.Now);
    private const string ValidNationalId = "1234567890";
    private const string ValidPostalCode = "1234567890";
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    public UpdateWorkshopCommandHandlerTests()
    {
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _workshopQuery = Substitute.For<IWorkshopQuery>();
        _workshopBuilder = new WorkshopBuilder();

        _handler = new UpdateWorkshopCommandHandler(_workShopRepository, _workshopQuery);
    }

    private Workshop CreateValidWorkshop()
    {
        return _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private UpdateWorkshopCommand CreateValidCommand(string? name = null, string? nationalId = null)
    {
        return new UpdateWorkshopCommand(ValidUserId,
            ValidWorkshopId,
            name ?? ValidName,
            ValidAddress,
            ValidRegistrationDate,
            nationalId ?? ValidNationalId,
            "1234567890",
            ValidPostalCode);
    }

    private void SetupNoDuplicates()
    {
        _workshopQuery.IsExistWorkshopName(ValidUserId, Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _workshopQuery.IsExistWorkshopNationalId(ValidUserId, Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateWorkshopAndReturnTrue()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenWorkshopNameIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workshopQuery.IsExistWorkshopName(ValidUserId, ValidName, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenWorkshopNationalIdIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workshopQuery.IsExistWorkshopName(ValidUserId, ValidName, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(false);
            
        _workshopQuery.IsExistWorkshopNationalId(ValidUserId, ValidNationalId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(name: "");
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
        var command = CreateValidCommand();
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
        var command = CreateValidCommand();
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
        var command = CreateValidCommand();
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
        var command = CreateValidCommand();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldNotCallUpdateAsync()
    {
        var command = CreateValidCommand(name: "");
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        SetupNoDuplicates();

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameIsDuplicate_ShouldNotCallUpdateAsync()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workshopQuery.IsExistWorkshopName(ValidUserId, ValidName, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNationalIdIsDuplicate_ShouldNotCallUpdateAsync()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workshopQuery.IsExistWorkshopName(ValidUserId, ValidName, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(false);
        _workshopQuery.IsExistWorkshopNationalId(ValidUserId, ValidNationalId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldNotCallIsExistWorkshopName()
    {
        var command = CreateValidCommand();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        // ✅ هماهنگی با ۴ پارامتر جدید متد IsExist
        await _workshopQuery.DidNotReceive()
            .IsExistWorkshopName(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldNotCallIsExistWorkshopNationalId()
    {
        var command = CreateValidCommand();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        // ✅ هماهنگی با ۴ پارامتر جدید متد IsExist
        await _workshopQuery.DidNotReceive()
            .IsExistWorkshopNationalId(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
