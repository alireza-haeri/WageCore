using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Features.EmployeeSalaryProfiles.Command.CreateEmployeeSalaryProfile;

public class CreateEmployeeSalaryProfileCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeSalaryProfileRepository _employeeSalaryProfileRepository;
    private readonly IEmployeeSalaryProfileQuery _employeeSalaryProfileQuery;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly CreateEmployeeSalaryProfileCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;
    private readonly EmployeeSalaryProfileBuilder _salaryProfileBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private const decimal ValidMinimumMonthlySalary = 71_661_840m;

    public CreateEmployeeSalaryProfileCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _employeeSalaryProfileRepository = Substitute.For<IEmployeeSalaryProfileRepository>();
        _employeeSalaryProfileQuery = Substitute.For<IEmployeeSalaryProfileQuery>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _employeeBuilder = new EmployeeBuilder();
        _salaryProfileBuilder = new EmployeeSalaryProfileBuilder();
        var logger = new Logger<CreateEmployeeSalaryProfileCommandHandler>(NullLoggerFactory.Instance);
        _handler = new CreateEmployeeSalaryProfileCommandHandler(
            _employeeRepository,
            _employeeSalaryProfileRepository,
            _employeeSalaryProfileQuery,
            _laborLawRuleQuery,
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

    private CreateEmployeeSalaryProfileCommand CreateValidCommand(
        EmployeeSalaryProfileDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null)
    {
        var dto = salaryProfile ?? _salaryProfileBuilder
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary)
            .WithEffectiveFrom(DateOnly.FromDateTime(DateTime.Now.AddDays(-5)))
            .BuildDto();

        return new CreateEmployeeSalaryProfileCommand(
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

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateSalaryProfileAndReturnId()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var createdId = Guid.NewGuid();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileRepository.CreateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.EmployeeSalaryProfileId.Should().Be(createdId);

        await _employeeSalaryProfileRepository.Received(1).CreateAsync(
            Arg.Is<EmployeeSalaryProfile>(x =>
                x.EmployeeId == ValidEmployeeId &&
                x.BaseMonthlySalary == ValidMinimumMonthlySalary &&
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
        await _employeeSalaryProfileRepository.DidNotReceive()
            .CreateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMinimumMonthlySalaryNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary(null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("حداقل حقوق ماهانه یافت نشد.", BadResultType.NotFound);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .CreateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGetMinimumMonthlySalaryByEffectiveFrom()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileRepository.CreateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>())
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
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary)
            .WithEffectiveFrom(hireDate.AddDays(-1))
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee(hireDate: hireDate);

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از تاریخ استخدام کارمند باشد.", BadResultType.General);
        await _employeeSalaryProfileRepository.DidNotReceive()
            .CreateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsBeforeExistingProfile_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var latestExisting = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
        var salaryProfile = _salaryProfileBuilder
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary)
            .WithEffectiveFrom(latestExisting.AddDays(-1))
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee(hireDate: hireDate);

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns(latestExisting);
        SetupMinimumMonthlySalary();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از پروفایل حقوق قبلی کارمند باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenBaseMonthlySalaryIsLessThanMinimum_ShouldReturnGeneralFailure()
    {
        var salaryProfile = _salaryProfileBuilder
            .WithBaseMonthlySalary(ValidMinimumMonthlySalary - 1)
            .BuildDto();
        var command = CreateValidCommand(salaryProfile);
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("حقوق پایه ماهانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryCreateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileRepository.CreateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>())
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
        _employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
                ValidUserId, ValidEmployeeId, null, Arg.Any<CancellationToken>())
            .Returns((DateOnly?)null);
        SetupMinimumMonthlySalary();
        _employeeSalaryProfileRepository.CreateAsync(Arg.Any<EmployeeSalaryProfile>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _employeeSalaryProfileRepository.Received(1).CreateAsync(
            Arg.Is<EmployeeSalaryProfile>(x =>
                x.BaseMonthlySalary >= ValidMinimumMonthlySalary),
            Arg.Any<CancellationToken>());
    }
}
