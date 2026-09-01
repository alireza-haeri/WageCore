using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Features.SalaryDecrees.Command.CreateSalaryDecree;

public class CreateSalaryDecreeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ISalaryDecreeRepository _salaryDecreeRepository;
    private readonly ISalaryDecreeQuery _salaryDecreeQuery;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly CreateSalaryDecreeCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;
    private readonly SalaryDecreeBuilder _salaryProfileBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private const decimal ValidMinimumMonthlySalary = 71_661_840m;

    public CreateSalaryDecreeCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _salaryDecreeRepository = Substitute.For<ISalaryDecreeRepository>();
        _salaryDecreeQuery = Substitute.For<ISalaryDecreeQuery>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _employeeBuilder = new EmployeeBuilder();
        _salaryProfileBuilder = new SalaryDecreeBuilder();
        var logger = new Logger<CreateSalaryDecreeCommandHandler>(NullLoggerFactory.Instance);
        _handler = new CreateSalaryDecreeCommandHandler(
            _employeeRepository,
            _salaryDecreeRepository,
            _salaryDecreeQuery,
            _laborLawRuleQuery,
            _payrollRecordQuery,
            logger);
    }

    private Employee CreateValidEmployee(Guid? employeeId = null, DateOnly? hireDate = null)
    {
        return _employeeBuilder
            .WithId(employeeId ?? ValidEmployeeId)
            .WithHireDate(hireDate ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
            .CreateResult()
            .ShouldBeSuccess();
    }

    private CreateSalaryDecreeCommand CreateValidCommand(
        SalaryDecreeDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null)
    {
        var dto = salaryProfile ?? _salaryProfileBuilder
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .WithEffectiveFrom(DateOnly.FromDateTime(DateTime.Now.AddDays(-5)))
            .BuildDto();

        return new CreateSalaryDecreeCommand(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            dto);
    }

    private void SetupMinimumMonthlySalary(decimal? value = ValidMinimumMonthlySalary)
    {
        _laborLawRuleQuery.GetActiveValueAsync(
                LaborLawRuleKey.MinimumMonthlySalary,
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(value);
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

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateSalaryProfileAndReturnId()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var createdId = Guid.NewGuid();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        SetupNoPayrollRecordEffect();
        _salaryDecreeRepository.CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.SalaryDecreeId.Should().Be(createdId);

        await _salaryDecreeRepository.Received(1).CreateAsync(
            Arg.Is<SalaryDecree>(x =>
                x.EmployeeId == ValidEmployeeId &&
                x.BaseDailySalary == ValidMinimumMonthlySalary &&
                x.EffectiveFrom == command.SalaryProfile.EffectiveFrom),
            Arg.Any<CancellationToken>());
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
            .CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMinimumMonthlySalaryNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary(null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("حداقل حقوق ماهانه یافت نشد.", BadResultType.NotFound);
        await _salaryDecreeRepository.DidNotReceive()
            .CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGetMinimumMonthlySalaryByEffectiveFrom()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _salaryDecreeRepository.CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _laborLawRuleQuery.Received(1).GetActiveValueAsync(
            LaborLawRuleKey.MinimumMonthlySalary,
            command.SalaryProfile.EffectiveFrom!.Value,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsBeforeHireDate_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var salaryProfile = _salaryProfileBuilder
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .WithEffectiveFrom(hireDate.AddDays(-1))
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee(hireDate: hireDate);

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از تاریخ استخدام کارمند باشد.", BadResultType.General);
        await _salaryDecreeRepository.DidNotReceive()
            .CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsBeforeExistingProfile_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var latestExisting = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
        var salaryProfile = _salaryProfileBuilder
            .WithBaseDailySalary(ValidMinimumMonthlySalary)
            .WithEffectiveFrom(latestExisting.AddDays(-1))
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee(hireDate: hireDate);

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns(latestExisting);
        SetupMinimumMonthlySalary();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از پروفایل حقوق قبلی کارمند باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenBaseDailySalaryIsLessThanMinimum_ShouldReturnGeneralFailure()
    {
        var salaryProfile = _salaryProfileBuilder
            .WithBaseDailySalary(ValidMinimumMonthlySalary - 1)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("حقوق پایه روزانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryCreateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _salaryDecreeRepository.CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldPassMinimumMonthlySalaryFromLaborLawToDomain()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _salaryDecreeRepository.CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _salaryDecreeRepository.Received(1).CreateAsync(
            Arg.Is<SalaryDecree>(x =>
                x.BaseDailySalary >= ValidMinimumMonthlySalary),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCheckPayrollRecordEffectByEffectiveFrom()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        SetupNoPayrollRecordEffect();
        _salaryDecreeRepository.CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _payrollRecordQuery.Received(1).HasPayrollRecordEffectAsync(
            ValidUserId,
            ValidEmployeeId,
            command.SalaryProfile.EffectiveFrom!.Value,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileHasPayrollRecordEffect_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _salaryDecreeQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _payrollRecordQuery.HasPayrollRecordEffectAsync(
                ValidUserId,
                ValidEmployeeId,
                command.SalaryProfile.EffectiveFrom!.Value,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(
            "امکان انتقال این حکم به این بازه وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.",
            BadResultType.General);
        await _salaryDecreeRepository.DidNotReceive()
            .CreateAsync(Arg.Any<SalaryDecree>(), Arg.Any<CancellationToken>());
    }
}
