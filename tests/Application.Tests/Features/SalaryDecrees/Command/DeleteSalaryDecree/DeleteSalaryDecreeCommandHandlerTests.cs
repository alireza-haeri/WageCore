namespace Application.Tests.Features.SalaryDecrees.Command.DeleteSalaryDecree;

public class DeleteSalaryDecreeCommandHandlerTests
{
    private readonly ISalaryDecreeRepository _salaryDecreeRepository;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly DeleteSalaryDecreeCommandHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));

    public DeleteSalaryDecreeCommandHandlerTests()
    {
        _salaryDecreeRepository = Substitute.For<ISalaryDecreeRepository>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _handler = new DeleteSalaryDecreeCommandHandler(
            _salaryDecreeRepository,
            _payrollRecordQuery);
    }

    private SalaryDecree CreateValidSalaryProfile(
        Guid? salaryProfileId = null,
        Guid? employeeId = null,
        DateOnly? effectiveFrom = null)
    {
        return new SalaryDecreeBuilder()
            .WithId(salaryProfileId ?? ValidSalaryProfileId)
            .WithEmployeeId(employeeId ?? ValidEmployeeId)
            .WithEmployeeHireDate(ValidHireDate)
            .WithEffectiveFrom(effectiveFrom ?? ValidEffectiveFrom)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private static DeleteSalaryDecreeCommand CreateValidCommand(
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? salaryProfileId = null) =>
        new(userId ?? ValidUserId, employeeId ?? ValidEmployeeId, salaryProfileId ?? ValidSalaryProfileId);

    [Fact]
    public async Task Handle_WithValidData_ShouldDeleteSalaryProfileAndReturnTrue()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile();

        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
        _salaryDecreeRepository.DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        await _salaryDecreeRepository.Received(1)
            .DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((SalaryDecree?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _salaryDecreeRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileBelongsToDifferentEmployee_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile(employeeId: Guid.NewGuid());

        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _salaryDecreeRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileHasPayrollRecordEffect_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile();

        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                ValidUserId,
                ValidEmployeeId,
                ValidEffectiveFrom,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("امکان حذف این حکم وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.", BadResultType.General);
        await _salaryDecreeRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCheckPayrollRecordEffectByProfileEffectiveFrom()
    {
        var command = CreateValidCommand();
        var profile = CreateValidSalaryProfile();

        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
        _salaryDecreeRepository.DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
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

        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
        _salaryDecreeRepository.DeleteAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
