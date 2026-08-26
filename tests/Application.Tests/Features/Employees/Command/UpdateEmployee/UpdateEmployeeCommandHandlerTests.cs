namespace Application.Tests.Features.Employees.Command.UpdateEmployee;

public class UpdateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeQuery _employeeQuery;
    private readonly IWorkShopRepository _workshopRepository;
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
        _employeeBuilder = new EmployeeBuilder();
        _workshopBuilder = new WorkshopBuilder();
        _handler = new UpdateEmployeeCommandHandler(_employeeRepository, _employeeQuery, _workshopRepository);
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
        {
            employee.CreateBankAccount(Guid.NewGuid(), new EmployeeBankAccountDto("حساب اول", "IR111111111111111111111111"))
                .ShouldBeSuccess();
            employee.CreateBankAccount(Guid.NewGuid(), new EmployeeBankAccountDto("حساب دوم", "IR222222222222222222222222"))
                .ShouldBeSuccess();
        }

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

    private UpdateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null, EmployeeInsuranceDto? insurance = null,
        List<EmployeeBankAccountDto>? bankAccounts = null)
    {
        var employeeDto = employee ?? _employeeBuilder
            .WithDepartmentId(UpdatedDepartmentId)
            .WithPersonalCode("EMP777")
            .WithNationalCode("0987654321")
            .WithBirthCertificateNumber("54321")
            .WithFatherName("محمود")
            .WithGender(EmployeeGender.Woman)
            .WithMaritalStatus(EmployeeMaritalStatus.Married)
            .WithChildrenCount(2)
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)))
            .WithPhoneNumber("09987654321")
            .WithJobTitle("سرپرست")
            .WithIsTaxSubject(false)
            .BuildEmployeeDto();

        var insuranceDto = insurance ?? _employeeBuilder
            .WithInsuranceNumber("INS-999")
            .WithSocialSecurityContractRow("CTR-99")
            .WithPositionInInsuranceList("مدیر مالی")
            .WithIsSubjectTo7PercentInsurance(false)
            .WithIsSubjectTo20PercentInsurance(true)
            .WithIsSubjectTo3PercentInsurance(true)
            .WithInsuranceCalculationProfile(InsuranceCalculationProfile.MinimumLaborLaw)
            .BuildInsuranceDto();

        var bankAccountDtos = bankAccounts ??
            [
                new EmployeeBankAccountDto("حساب اول جدید", "IR999999999999999999999999"),
                new EmployeeBankAccountDto("حساب دوم جدید", "IR888888888888888888888888")
            ];

        return new UpdateEmployeeCommand(ValidUserId, ValidEmployeeId, employeeDto, insuranceDto, bankAccountDtos);
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
            new EmployeeBankAccountDto("حساب اول جدید", "IR999999999999999999999999", existingBankAccountIds[0]),
            new EmployeeBankAccountDto("حساب سوم", "IR777777777777777777777777")
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
            employee.Insurance.InsuranceNumber.Should().Be(command.Insurance!.InsuranceNumber);
            employee.Insurance.PositionInInsuranceList.Should().Be(command.Insurance.PositionInInsuranceList);
            employee.Insurance.InsuranceCalculationProfile.Should().Be(command.Insurance.InsuranceCalculationProfile!.Value);
            employee.BankAccounts.Should().HaveCount(2);
            employee.BankAccounts.Should().Contain(x => x.Id == existingBankAccountIds[0] && x.Title == "حساب اول جدید" && x.Iban == "999999999999999999999999");
            employee.BankAccounts.Should().Contain(x => x.Title == "حساب سوم" && x.Iban == "777777777777777777777777");
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
        employee.BankAccounts.Should().Contain(x => x.Title == command.BankAccounts![0].Title && x.Iban == command.BankAccounts[0].Iban[2..]);
        employee.BankAccounts.Should().Contain(x => x.Title == command.BankAccounts[1].Title && x.Iban == command.BankAccounts[1].Iban[2..]);
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
    public async Task Handle_WhenInsuranceUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(insurance: new EmployeeInsuranceDto(
            string.Empty,
            "CTR-99",
            "مدیر مالی",
            false,
            true,
            true,
            InsuranceCalculationProfile.MinimumLaborLaw));
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

        result.ShouldBeFailure("شماره بیمه نمیتواند خالی باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenBankAccountsReplaceFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(bankAccounts:
        [
            new EmployeeBankAccountDto("حساب اول", "IR999999999999999999999999"),
            new EmployeeBankAccountDto("حساب دوم", "IR999999999999999999999999")
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
}
