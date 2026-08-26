namespace Application.Tests.Features.Employees.Command.CreateEmployee;

public class CreateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeQuery _employeeQuery;
    private readonly IWorkShopRepository _workshopRepository;
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
        _employeeBuilder = new EmployeeBuilder();
        _workshopBuilder = new WorkshopBuilder();
        _handler = new CreateEmployeeCommandHandler(_employeeRepository, _employeeQuery, _workshopRepository);
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

    private CreateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null, EmployeeInsuranceDto? insurance = null,
        List<EmployeeBankAccountDto>? bankAccounts = null)
    {
        var employeeDto = employee ?? _employeeBuilder
            .WithWorkshopId(ValidWorkshopId)
            .WithDepartmentId(ValidDepartmentId)
            .BuildEmployeeDto();

        var insuranceDto = insurance ?? _employeeBuilder.BuildInsuranceDto();
        var bankAccountDtos = bankAccounts ??
            [
                _employeeBuilder.BuildBankAccountDto(),
                new EmployeeBankAccountDto("حساب دوم", "IR999999999999999999999999")
            ];

        return new CreateEmployeeCommand(ValidUserId, ValidWorkshopId, employeeDto, insuranceDto, bankAccountDtos);
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
        await _employeeRepository.Received(1).CreateAsync(
            Arg.Is<Employee>(x =>
                x.WorkshopId == ValidWorkshopId &&
                x.DepartmentId == ValidDepartmentId &&
                x.PersonalCode == command.Employee.PersonalCode &&
                x.NationalCode == command.Employee.NationalCode &&
                x.Insurance.InsuranceNumber == command.Insurance.InsuranceNumber &&
                x.BankAccounts.Count == 2 &&
                x.BankAccounts.Any(b => b.Title == command.BankAccounts![0].Title && b.Iban == command.BankAccounts[0].Iban[2..]) &&
                x.BankAccounts.Any(b => b.Title == command.BankAccounts[1].Title && b.Iban == command.BankAccounts[1].Iban[2..])),
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
            new EmployeeBankAccountDto("حساب دوم", "IR123456789012345678901234")
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
}
