namespace Application.Tests.Features.EmployeeSalaryProfiles.Command.DeleteEmployeeSalaryProfile;

public class DeleteEmployeeSalaryProfileCommandHandlerTests
{
    private readonly IEmployeeSalaryProfileRepository _employeeSalaryProfileRepository;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly DeleteEmployeeSalaryProfileCommandHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));

    public DeleteEmployeeSalaryProfileCommandHandlerTests()
    {
        _employeeSalaryProfileRepository = Substitute.For<IEmployeeSalaryProfileRepository>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _handler = new DeleteEmployeeSalaryProfileCommandHandler(
            _employeeSalaryProfileRepository,
            _payrollRecordQuery);
    }

    private EmployeeSalaryProfile CreateValidSalaryProfile(
        Guid? salaryProfileId = null,
        Guid? employeeId = null,
        DateOnly? effectiveFrom = null)
    {
        return new EmployeeSalaryProfileBuilder()
            .WithId(salaryProfileId ?? ValidSalaryProfileId)
            .WithEmployeeId(employeeId ?? ValidEmployeeId)
            .WithEmployeeHireDate(ValidHireDate)
            .WithEffectiveFrom(effectiveFrom ?? ValidEffectiveFrom)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private static DeleteEmployeeSalaryProfileCommand CreateValidCommand(
        Guid? userId = null,
        Guid? salaryProfileId = null) =>
        new(userId ?? ValidUserId, salaryProfileId ?? ValidSalaryProfileId);

    [Fact]
    public async Task Handle_WithValidData_ShouldDeleteSalaryProfileAndReturnTrue()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile();

        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
        _employeeSalaryProfileRepository.DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        await _employeeSalaryProfileRepository.Received(1)
            .DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((EmployeeSalaryProfile?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileHasPayrollRecordEffect_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile();

        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                ValidUserId,
                ValidEmployeeId,
                ValidEffectiveFrom,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("این پروفایل حقوق بر روی فیش حقوقی اثر دارد و امکان حذف آن وجود ندارد.", BadResultType.General);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCheckPayrollRecordEffectByProfileEffectiveFrom()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile();

        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
        _employeeSalaryProfileRepository.DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _payrollRecordQuery.Received(1).HasPayrollRecordEffectAsync(
            ValidUserId,
            ValidEmployeeId,
            ValidEffectiveFrom,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryDeleteFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile();

        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
        _employeeSalaryProfileRepository.DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
