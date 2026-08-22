namespace Application.Tests.Features.Workshops.Command.CreateWorkshop;

public class CreateWorkshopCommandHandlerTests
{
    private readonly IWorkShopRepository _workShopRepository;
    private readonly CreateWorkshopCommandHandler _handler;
    private readonly WorkshopBuilder _workshopBuilder;

    private const string ValidName = "کارگاه نمونه";
    private const string ValidAddress = "تهران، خیابان نمونه، پلاک ۱۲۳";
    private const WorkshopRegion ValidRegion = WorkshopRegion.Normal;
    private static readonly DateOnly ValidRegistrationDate = DateOnly.FromDateTime(DateTime.Now);
    private const string ValidNationalId = "1234567890";
    private const string ValidPostalCode = "1234567890";
    private static readonly Guid ValidUserId = Guid.NewGuid();

    public CreateWorkshopCommandHandlerTests()
    {
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _workshopBuilder = new WorkshopBuilder();

        _handler = new CreateWorkshopCommandHandler(_workShopRepository);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateWorkshopAndReturnId()
    {
        var command = new CreateWorkshopCommand(
            ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.CreateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>())
            .Returns(workshop.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.WorkshopId.Should().Be(workshop.Id);
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnGeneralFailure()
    {
        var invalidName = "";
        var command = new CreateWorkshopCommand(
            ValidUserId,
            invalidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryCreateFails_ShouldReturnGeneralFailure()
    {
        var command = new CreateWorkshopCommand(
            ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        _workShopRepository.CreateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallCreateAsyncOnce()
    {
        var command = new CreateWorkshopCommand(
            ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        var workshop = _workshopBuilder
            .WithUserId(ValidUserId)
            .WithName(ValidName)
            .WithAddress(ValidAddress)
            .WithRegion(ValidRegion)
            .WithRegistrationDate(ValidRegistrationDate)
            .WithNationalId(ValidNationalId)
            .WithPostalCode(ValidPostalCode)
            .CreateResult()
            .ShouldBeSuccess();

        _workShopRepository.CreateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>())
            .Returns(workshop.Id);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.Received(1).CreateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainFails_ShouldNotCallCreateAsync()
    {
        var invalidName = "";
        var command = new CreateWorkshopCommand(
            ValidUserId,
            invalidName,
            ValidAddress,
            ValidRegion,
            ValidRegistrationDate,
            ValidNationalId,
            ValidPostalCode);

        await _handler.Handle(command, CancellationToken.None);

        await _workShopRepository.DidNotReceive().CreateAsync(Arg.Any<Workshop>(), Arg.Any<CancellationToken>());
    }
}