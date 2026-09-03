namespace Application.Tests.Features.Employees.Command.UpdateEmployee;

public class UpdateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeQuery _employeeQuery;
    private readonly IWorkShopRepository _workshopRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly UpdateEmployeeCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;
    private readonly WorkshopBuilder _workshopBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private static readonly Guid UpdatedDepartmentId = Guid.NewGuid();
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    public UpdateEmployeeCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _employeeQuery = Substitute.For<IEmployeeQuery>();
        _workshopRepository = Substitute.For<IWorkShopRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _employeeBuilder = new EmployeeBuilder();
        _workshopBuilder = new WorkshopBuilder();

        // Treat every date as the current Persian month so the default
        // builder data (hired a few days ago, no history fields) stays valid.
        _persianCalendarService.GetPersianYear(Arg.Any<DateOnly>()).Returns(1405);
        _persianCalendarService.GetPersianMonth(Arg.Any<DateOnly>()).Returns(6);

        _handler = new UpdateEmployeeCommandHandler(
            _employeeRepository,
            _employeeQuery,
            _workshopRepository,
            _persianCalendarService);
    }

    private Employee CreateValidEmployee(bool createBankAccounts = false)
    {
        var employee = _employeeBuilder
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithDepartmentId(ValidDepartmentId)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .CreateResult()
            .ShouldBeSuccess();

        if (createBankAccounts)
            employee.ReplaceBankAccounts([
                new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR111111111111111111111111", Guid.NewGuid()),
                new EmployeeBankAccountDto("بانک صادرات", "۳۰۳", "IR222222222222222222222222", Guid.NewGuid())
            ]).ShouldBeSuccess();

        return employee;
    }

    private Workshop CreateValidWorkshop(Guid? workshopId = null, DateOnly? registrationDate = null)
    {
        return _workshopBuilder
            .WithId(workshopId ?? ValidWorkshopId)
            .WithUserId(ValidUserId)
            .WithRegistrationDate(registrationDate ?? ValidWorkshopRegistrationDate)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private UpdateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null,
        List<EmployeeBankAccountDto>? bankAccounts = null)
    {
        var employeeDto = employee ?? _employeeBuilder
            .WithDepartmentId(UpdatedDepartmentId)
            .WithPersonalCode("EMP777")
            .WithNationalCode("0987654321")
            .WithFatherName("محمود")
            .WithGender(EmployeeGender.Woman)
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)))
            .WithPhoneNumber("09987654321")
            .WithJobTitle("سرپرست")
            .BuildEmployeeDto();

        var bankAccountDtos = bankAccounts ??
            [
                new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR999999999999999999999999"),
                new EmployeeBankAccountDto("بانک صادرات", "۳۰۳", "IR888888888888888888888888")
            ];

        return new UpdateEmployeeCommand(ValidUserId, ValidEmployeeId, employeeDto, bankAccountDtos);
    }

    private void SetupNoDuplicates()
    {
        _employeeQuery.IsExistEmployeePersonalCode(ValidUserId, Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _employeeQuery.IsExistEmployeeNationalCode(ValidUserId, Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateEmployeeAndReturnTrue()
    {
        var employee = CreateValidEmployee(createBankAccounts: true);
        var existingBankAccountIds = employee.BankAccounts.Select(x => x.Id).ToList();
        var command = CreateValidCommand(bankAccounts:
        [
            new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR999999999999999999999999", existingBankAccountIds[0]),
            new EmployeeBankAccountDto("بانک ملت", "۴۰۴", "IR777777777777777777777777")
        ]);
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            employee.DepartmentId.Should().Be(UpdatedDepartmentId);
            employee.PersonalCode.Should().Be(command.Employee.PersonalCode);
            employee.NationalCode.Should().Be(command.Employee.NationalCode);
            employee.BankAccounts.Should().HaveCount(2);
            employee.BankAccounts.Should().Contain(x => x.Id == existingBankAccountIds[0] && x.BankName == "بانک ملی" && x.Iban == "999999999999999999999999");
            employee.BankAccounts.Should().Contain(x => x.BankName == "بانک ملت" && x.Iban == "777777777777777777777777");
            employee.BankAccounts.Should().NotContain(x => x.Id == existingBankAccountIds[1]);
        }
    }

    [Fact]
    public async Task Handle_WhenEmployeeHasNoBankAccounts_ShouldReplaceWithNewBankAccounts()
    {
        var employee = CreateValidEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeSuccess().Should().BeTrue();
        employee.BankAccounts.Should().HaveCount(2);
        var accounts = command.BankAccounts!;

        var firstBankName = accounts[0].BankName;
        var firstBranchCode = accounts[0].BranchCode;
        var firstIban = accounts[0].Iban[2..];
        var secondBankName = accounts[1].BankName;
        var secondBranchCode = accounts[1].BranchCode;
        var secondIban = accounts[1].Iban[2..];

        employee.BankAccounts.Should().Contain(x => x.BankName == firstBankName && x.BranchCode == firstBranchCode && x.Iban == firstIban);
        employee.BankAccounts.Should().Contain(x => x.BankName == secondBankName && x.BranchCode == secondBranchCode && x.Iban == secondIban);
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDepartmentBelongsToAnotherWorkshop_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();
        var otherWorkshop = CreateValidWorkshop(Guid.NewGuid());

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(otherWorkshop);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPersonalCodeIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeQuery.IsExistEmployeePersonalCode(ValidUserId, command.Employee.PersonalCode, ValidEmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenNationalCodeIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeQuery.IsExistEmployeePersonalCode(ValidUserId, command.Employee.PersonalCode, ValidEmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(false);
        _employeeQuery.IsExistEmployeeNationalCode(ValidUserId, command.Employee.NationalCode, ValidEmployeeId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenBankAccountsReplaceFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(bankAccounts:
        [
            new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR999999999999999999999999"),
            new EmployeeBankAccountDto("بانک صادرات", "۳۰۳", "IR999999999999999999999999")
        ]);
        var employee = CreateValidEmployee(createBankAccounts: true);
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("شماره شبا در لیست حساب‌های بانکی تکراری است.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WithEmptyBankAccounts_ShouldReturnGeneralFailureAndKeepExistingBankAccounts()
    {
        var command = CreateValidCommand(bankAccounts: []);
        var employee = CreateValidEmployee(createBankAccounts: true);
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("کارمند باید حداقل یک حساب بانکی داشته باشد.", BadResultType.General);
        employee.BankAccounts.Should().HaveCount(2);
        await _employeeRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsyncOnce()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.Received(1).GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncOnce()
    {
        var command = CreateValidCommand();
        var employee = CreateValidEmployee();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.Received(1).UpdateAsync(employee, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldNotCallUpdateAsync()
    {
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldNotCallDuplicateChecks()
    {
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeQuery.DidNotReceive()
            .IsExistEmployeePersonalCode(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await _employeeQuery.DidNotReceive()
            .IsExistEmployeeNationalCode(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdatedHireDateIsEarlierThisYear_WithoutHistoryFields_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-60));
        _persianCalendarService.GetPersianMonth(hireDate).Returns(3);

        var employee = CreateValidEmployee();
        var employeeDto = _employeeBuilder
            .WithDepartmentId(UpdatedDepartmentId)
            .WithHireDate(hireDate)
            .BuildEmployeeDto();
        var command = CreateValidCommand(employee: employeeDto);
        var workshop = CreateValidWorkshop(registrationDate: DateOnly.FromDateTime(DateTime.Now.AddDays(-100)));

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("مرخصی استفاده‌شده در سال جاری اجباری", BadResultType.General);
        await _employeeRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdatedHireDateIsEarlierThisYear_WithHistoryFields_ShouldUpdateEmployee()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-60));
        _persianCalendarService.GetPersianMonth(hireDate).Returns(3);

        var employee = CreateValidEmployee(createBankAccounts: true);
        var existingBankAccountIds = employee.BankAccounts.Select(x => x.Id).ToList();
        var employeeDto = _employeeBuilder
            .WithDepartmentId(UpdatedDepartmentId)
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .BuildEmployeeDto();
        var command = CreateValidCommand(
            employee: employeeDto,
            bankAccounts: [new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR999999999999999999999999", existingBankAccountIds[0])]);
        var workshop = CreateValidWorkshop(registrationDate: DateOnly.FromDateTime(DateTime.Now.AddDays(-100)));

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, UpdatedDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            employee.LeaveUsedInCurrentYear.Should().Be(3);
            employee.NetWorkedDaysBeforeCurrentMonth.Should().Be(45);
            employee.CarriedOverLeaveFromPreviousYear.Should().BeNull();
        }
    }
}
