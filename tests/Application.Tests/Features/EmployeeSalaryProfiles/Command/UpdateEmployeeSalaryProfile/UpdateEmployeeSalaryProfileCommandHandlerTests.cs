using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Features.EmployeeSalaryProfiles.Command.UpdateEmployeeSalaryProfile;

public class UpdateEmployeeSalaryProfileCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeSalaryProfileRepository _employeeSalaryProfileRepository;
    private readonly IEmployeeSalaryProfileQuery _employeeSalaryProfileQuery;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly UpdateEmployeeSalaryProfileCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;
    private readonly EmployeeSalaryProfileBuilder _salaryProfileBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
    private const decimal ValidMinimumMonthlySalary = 71_661_840m;

    public UpdateEmployeeSalaryProfileCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _employeeSalaryProfileRepository = Substitute.For<IEmployeeSalaryProfileRepository>();
        _employeeSalaryProfileQuery = Substitute.For<IEmployeeSalaryProfileQuery>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _employeeBuilder = new EmployeeBuilder();
        _salaryProfileBuilder = new EmployeeSalaryProfileBuilder();
        var logger = new Logger<UpdateEmployeeSalaryProfileCommandHandler>(NullLoggerFactory.Instance);
        _handler = new UpdateEmployeeSalaryProfileCommandHandler(
            _employeeRepository,
            _employeeSalaryProfileRepository,
            _employeeSalaryProfileQuery,
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
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private UpdateEmployeeSalaryProfileCommand CreateValidCommand(
        EmployeeSalaryProfileDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? salaryProfileId = null)
    {
        var dto = salaryProfile ?? _salaryProfileBuilder
            .WithEffectiveFrom(ValidEffectiveFrom)
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary)
            .BuildDto();

        return new UpdateEmployeeSalaryProfileCommand(
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
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _employeeSalaryProfileRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            profile.EffectiveFrom.Should().Be(newEffectiveFrom);
            profile.BaseMonthlySalary.Should().Be(ValidMinimumMonthlySalary);
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
        await _employeeSalaryProfileRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((EmployeeSalaryProfile?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileBelongsToDifferentEmployee_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile(employeeId: Guid.NewGuid());

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileHasPayrollRecordEffect_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                ValidUserId,
                ValidEmployeeId,
                command.SalaryProfile.EffectiveFrom!.Value,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("این پروفایل حقوق بر روی فیش حقوقی اثر دارد و امکان ویرایش آن وجود ندارد.", BadResultType.General);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCheckPayrollRecordEffectByEffectiveFrom()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _employeeSalaryProfileRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _payrollRecordQuery.Received(1).HasPayrollRecordEffectAsync(
            ValidUserId,
            ValidEmployeeId,
            command.SalaryProfile.EffectiveFrom!.Value,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGetLatestEffectiveFromExcludingCurrentProfile()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _employeeSalaryProfileRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetLatestEffectiveFromAsync(
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
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary(null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("حداقل حقوق ماهانه یافت نشد.", BadResultType.NotFound);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsBeforeHireDate_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var salaryProfile = _salaryProfileBuilder
            .WithEffectiveFrom(hireDate.AddDays(-1))
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee(hireDate: hireDate);
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از تاریخ استخدام کارمند باشد.", BadResultType.General);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var profile = CreateValidSalaryProfile();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileRepository.GetByIdAsync(ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(profile);
        SetupNoPayrollRecordEffect();
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        _employeeSalaryProfileRepository.UpdateAsync(profile, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
