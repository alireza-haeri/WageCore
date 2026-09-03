namespace Application.Tests.Features.Employees.Command.CreateEmployee;

public class CreateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeQuery _employeeQuery;
    private readonly IWorkShopRepository _workshopRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly CreateEmployeeCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;
    private readonly WorkshopBuilder _workshopBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    public CreateEmployeeCommandHandlerTests()
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

        _handler = new CreateEmployeeCommandHandler(
            _employeeRepository,
            _employeeQuery,
            _workshopRepository,
            _persianCalendarService);
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

    private CreateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null,
        List<EmployeeBankAccountDto>? bankAccounts = null)
    {
        var employeeDto = employee ?? _employeeBuilder
            .WithWorkshopId(ValidWorkshopId)
            .WithDepartmentId(ValidDepartmentId)
            .BuildEmployeeDto();

        var bankAccountDtos = bankAccounts ??
            [
                _employeeBuilder.BuildBankAccountDto(),
                new EmployeeBankAccountDto("بانک صادرات", "۳۰۳", "IR999999999999999999999999")
            ];

        return new CreateEmployeeCommand(ValidUserId, ValidWorkshopId, employeeDto, bankAccountDtos);
    }

    private void SetupNoDuplicates()
    {
        _employeeQuery.IsExistEmployeePersonalCode(ValidUserId, Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(false);
        _employeeQuery.IsExistEmployeeNationalCode(ValidUserId, Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateEmployeeAndReturnId()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();
        var createdEmployeeId = Guid.NewGuid();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        _employeeRepository.CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>())
            .Returns(createdEmployeeId);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.EmployeeId.Should().Be(createdEmployeeId);
        var accounts = command.BankAccounts!;

        var firstBankName = accounts[0].BankName;
        var firstBranchCode = accounts[0].BranchCode;
        var firstIban = accounts[0].Iban[2..];
        var secondBankName = accounts[1].BankName;
        var secondBranchCode = accounts[1].BranchCode;
        var secondIban = accounts[1].Iban[2..];

        await _employeeRepository.Received(1).CreateAsync(
            Arg.Is<Employee>(x =>
                x.WorkshopId == ValidWorkshopId &&
                x.DepartmentId == ValidDepartmentId &&
                x.PersonalCode == command.Employee.PersonalCode &&
                x.NationalCode == command.Employee.NationalCode &&
                x.BankAccounts.Count == 2 &&
                x.BankAccounts.Any(b => b.BankName == firstBankName && b.BranchCode == firstBranchCode && b.Iban == firstIban) &&
                x.BankAccounts.Any(b => b.BankName == secondBankName && b.BranchCode == secondBranchCode && b.Iban == secondIban)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCheckPersonalCodeDuplicateByUserId()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        _employeeRepository.CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _employeeQuery.Received(1).IsExistEmployeePersonalCode(
            ValidUserId,
            command.Employee.PersonalCode,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDepartmentNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDepartmentBelongsToAnotherWorkshop_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();
        var otherWorkshop = CreateValidWorkshop(Guid.NewGuid());

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(otherWorkshop);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPersonalCodeIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeQuery.IsExistEmployeePersonalCode(ValidUserId, command.Employee.PersonalCode, null,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenNationalCodeIsDuplicate_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeQuery.IsExistEmployeePersonalCode(ValidUserId, command.Employee.PersonalCode, null,
                Arg.Any<CancellationToken>())
            .Returns(false);
        _employeeQuery.IsExistEmployeeNationalCode(ValidUserId, command.Employee.NationalCode, null,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenHireDateIsBeforeWorkshopRegistrationDate_ShouldReturnGeneralFailure()
    {
        var employee = _employeeBuilder
            .WithDepartmentId(ValidDepartmentId)
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-20)))
            .BuildEmployeeDto();
        var command = CreateValidCommand(employee: employee);
        var workshop = CreateValidWorkshop(registrationDate: DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ استخدام نباید قبل از تاریخ ثبت کارگاه باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnGeneralFailure()
    {
        var employee = _employeeBuilder
            .WithDepartmentId(ValidDepartmentId)
            .WithFullName(string.Empty)
            .BuildEmployeeDto();
        var command = CreateValidCommand(employee: employee);
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenBankAccountsReplaceFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(bankAccounts:
        [
            _employeeBuilder.BuildBankAccountDto(),
            new EmployeeBankAccountDto("بانک صادرات", "۳۰۳", "IR123456789012345678901234")
        ]);
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("شماره شبا در لیست حساب‌های بانکی تکراری است.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WithEmptyBankAccounts_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(bankAccounts: []);
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("کارمند باید حداقل یک حساب بانکی داشته باشد.", BadResultType.General);
        await _employeeRepository.DidNotReceive().CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryCreateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        _employeeRepository.CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldNotCallCreateAsync()
    {
        var command = CreateValidCommand();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.DidNotReceive().CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPersonalCodeIsDuplicate_ShouldNotCallCreateAsync()
    {
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeQuery.IsExistEmployeePersonalCode(ValidUserId, command.Employee.PersonalCode, null,
                Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.DidNotReceive().CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenHiredEarlierThisYear_WithoutHistoryFields_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-60));
        _persianCalendarService.GetPersianMonth(hireDate).Returns(3);

        var employee = _employeeBuilder
            .WithDepartmentId(ValidDepartmentId)
            .WithHireDate(hireDate)
            .BuildEmployeeDto();
        var command = CreateValidCommand(employee: employee);
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("مرخصی استفاده‌شده در سال جاری اجباری", BadResultType.General);
        await _employeeRepository.DidNotReceive().CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenHiredEarlierThisYear_WithHistoryFields_ShouldCreateEmployee()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-60));
        _persianCalendarService.GetPersianMonth(hireDate).Returns(3);

        var employee = _employeeBuilder
            .WithDepartmentId(ValidDepartmentId)
            .WithHireDate(hireDate)
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .BuildEmployeeDto();
        var command = CreateValidCommand(employee: employee);
        var workshop = CreateValidWorkshop();
        var createdEmployeeId = Guid.NewGuid();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();
        _employeeRepository.CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>())
            .Returns(createdEmployeeId);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.EmployeeId.Should().Be(createdEmployeeId);
        await _employeeRepository.Received(1).CreateAsync(
            Arg.Is<Employee>(x =>
                x.LeaveUsedInCurrentYear == 3 &&
                x.NetWorkedDaysBeforeCurrentMonth == 45 &&
                x.CarriedOverLeaveFromPreviousYear is null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenHiredBeforeCurrentYear_WithoutCarriedOverLeave_ShouldReturnGeneralFailure()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-400));
        _persianCalendarService.GetPersianYear(hireDate).Returns(1404);

        var employee = _employeeBuilder
            .WithDepartmentId(ValidDepartmentId)
            .WithHireDate(hireDate)
            .WithWorkshopRegistrationDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-500)))
            .WithLeaveUsedInCurrentYear(3)
            .WithNetWorkedDaysBeforeCurrentMonth(45)
            .BuildEmployeeDto();
        var command = CreateValidCommand(employee: employee);
        var workshop = CreateValidWorkshop(registrationDate: DateOnly.FromDateTime(DateTime.Now.AddDays(-500)));

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("مرخصی انتقال‌یافته از سال قبل اجباری", BadResultType.General);
        await _employeeRepository.DidNotReceive().CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenHiredInCurrentMonth_WithHistoryFields_ShouldReturnGeneralFailure()
    {
        var employee = _employeeBuilder
            .WithDepartmentId(ValidDepartmentId)
            .WithLeaveUsedInCurrentYear(3)
            .BuildEmployeeDto();
        var command = CreateValidCommand(employee: employee);
        var workshop = CreateValidWorkshop();

        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, ValidDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        SetupNoDuplicates();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("همین ماه استخدام شده", BadResultType.General);
        await _employeeRepository.DidNotReceive().CreateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }
}
