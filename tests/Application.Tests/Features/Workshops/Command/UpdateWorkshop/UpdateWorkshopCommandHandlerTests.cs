namespace Application.Tests.Features.Workshops.Command.UpdateWorkshop;

public class UpdateWorkshopCommandHandlerTests
{
    private readonly IWorkShopRepository _workShopRepository;
    private readonly UpdateWorkshopCommandHandler _handler;
    private readonly WorkshopBuilder _workshopBuilder;

    private const string ValidName = "کارگاه نمونه";
    private const string ValidAddress = "تهران، خیابان نمونه، پلاک ۱۲۳";
    private const WorkshopRegion ValidRegion = WorkshopRegion.Normal;
    private static readonly DateOnly ValidRegistrationDate = DateOnly.FromDateTime(DateTime.Now);
    private const string ValidNationalId = "1234567890";
    private const string ValidPostalCode = "1234567890";
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    public UpdateWorkshopCommandHandlerTests()
    {
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _workshopBuilder = new WorkshopBuilder();

        _handler = new UpdateWorkshopCommandHandler(_workShopRepository);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateWorkshopAndReturnTrue()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldReturnGeneralFailure()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            "", // Name خالی باعث خطای دامنه می‌شود
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsyncOnce()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _workShopRepository.Received(1).GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncOnce()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        _workShopRepository.UpdateAsync(workshop, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _workShopRepository.Received(1).UpdateAsync(workshop, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldNotCallUpdateAsync()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldNotCallUpdateAsync()
    {
        // Arrange
        var command = new UpdateWorkshopCommand(
            ValidUserId,
            ValidWorkshopId,
            "", // Name خالی باعث خطای دامنه می‌شود
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _workShopRepository.DidNotReceive().UpdateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }
}