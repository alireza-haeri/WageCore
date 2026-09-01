using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Features.SalaryDecrees.Command.UpdateSalaryDecree;

public class UpdateSalaryDecreeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ISalaryDecreeRepository _salaryDecreeRepository;
    private readonly ISalaryDecreeQuery _salaryDecreeQuery;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly UpdateSalaryDecreeCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;
    private readonly SalaryDecreeBuilder _salaryProfileBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
    private const decimal ValidMinimumMonthlySalary = 71_661_840m;

    public UpdateSalaryDecreeCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _salaryDecreeRepository = Substitute.For<ISalaryDecreeRepository>();
        _salaryDecreeQuery = Substitute.For<ISalaryDecreeQuery>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _employeeBuilder = new EmployeeBuilder();
        _salaryProfileBuilder = new SalaryDecreeBuilder();
        var logger = new Logger<UpdateSalaryDecreeCommandHandler>(NullLoggerFactory.Instance);
        _handler = new UpdateSalaryDecreeCommandHandler(
            _employeeRepository,
            _salaryDecreeRepository,
            _salaryDecreeQuery,
            _payrollRecordQuery,
            _laborLawRuleQuery,
            logger);
    }

    private Employee CreateValidEmployee(DateOnly? hireDate = null)
    {
        return _employeeBuilder
            .WithId(ValidEmployeeId)
            .WithHireDate(hireDate ?? ValidHireDate)
            .CreateResult()
            .ShouldBeSuccess();
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
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private UpdateSalaryDecreeCommand CreateValidCommand(
        SalaryDecreeDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? salaryProfileId = null)
    {
        var dto = salaryProfile ?? _salaryProfileBuilder
            .WithEffectiveFrom(ValidEffectiveFrom)
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .BuildDto();

        return new UpdateSalaryDecreeCommand(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            salaryProfileId ?? ValidSalaryProfileId,
            dto);
    }

    private void SetupNoPayrollRecordEffect()
    {
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
    }

    private void SetupMinimumMonthlySalary(decimal? value = ValidMinimumMonthlySalary)
    {
        _laborLawRuleQuery.GetActiveValueAsync(
                LaborLawRuleKey.MinimumMonthlySalary,
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(value);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateSalaryProfileAndReturnTrue()
    {
        var newEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var salaryProfile = _salaryProfileBuilder
            .WithEffectiveFrom(newEffectiveFrom)
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _salaryDecreeRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            profile.EffectiveFrom.Should().Be(newEffectiveFrom);
            profile.BaseDailySalary.Should().Be(ValidMinimumMonthlySalary);
        }
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
        await _salaryDecreeRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((SalaryDecree?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _salaryDecreeRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileBelongsToDifferentEmployee_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile(employeeId: Guid.NewGuid());

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _salaryDecreeRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileHasPayrollRecordEffect_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                ValidUserId,
                ValidEmployeeId,
                command.SalaryProfile.EffectiveFrom!.Value,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("امکان ویرایش این حکم وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.", BadResultType.General);
        await _salaryDecreeRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromUnchanged_ShouldCheckOnlyExistingPeriod()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _salaryDecreeRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _payrollRecordQuery.Received(1).HasPayrollRecordEffectAsync(
            ValidUserId,
            ValidEmployeeId,
            profile.EffectiveFrom,
            Arg.Any<CancellationToken>());
        await _payrollRecordQuery.Received(1).HasPayrollRecordEffectAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<DateOnly>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromChanges_ShouldCheckExistingAndNewPeriod()
    {
        var newEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var salaryProfile = _salaryProfileBuilder
            .WithEffectiveFrom(newEffectiveFrom)
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _salaryDecreeRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _payrollRecordQuery.Received(1).HasPayrollRecordEffectAsync(
            ValidUserId,
            ValidEmployeeId,
            profile.EffectiveFrom,
            Arg.Any<CancellationToken>());
        await _payrollRecordQuery.Received(1).HasPayrollRecordEffectAsync(
            ValidUserId,
            ValidEmployeeId,
            newEffectiveFrom,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenChangingToAffectedPeriod_ShouldReturnGeneralFailure()
    {
        var newEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var salaryProfile = _salaryProfileBuilder
            .WithEffectiveFrom(newEffectiveFrom)
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                ValidUserId,
                ValidEmployeeId,
                profile.EffectiveFrom,
                Arg.Any<CancellationToken>())
            .Returns(false);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                ValidUserId,
                ValidEmployeeId,
                newEffectiveFrom,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("امکان انتقال این حکم به این بازه وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.", BadResultType.General);
        await _salaryDecreeRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGetLatestEffectiveFromExcludingCurrentProfile()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _salaryDecreeRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetLatestEffectiveFromAsync(
            ValidUserId,
            ValidEmployeeId,
            ValidSalaryProfileId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMinimumMonthlySalaryNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary(null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("حداقل حقوق ماهانه یافت نشد.", BadResultType.NotFound);
        await _salaryDecreeRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsBeforeHireDate_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var salaryProfile = _salaryProfileBuilder
            .WithEffectiveFrom(hireDate.AddDays(-1))
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee(hireDate: hireDate);
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از تاریخ استخدام کارمند باشد.", BadResultType.General);
        await _salaryDecreeRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _salaryDecreeRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
