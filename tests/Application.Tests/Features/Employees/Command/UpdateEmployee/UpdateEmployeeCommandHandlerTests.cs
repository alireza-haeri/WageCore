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

    private Employee CreateValidEmployee()
    {
        return _employeeBuilder
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithDepartmentId(ValidDepartmentId)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .CreateResult()
            .ShouldBeSuccess();
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

    private UpdateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null)
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

        return new UpdateEmployeeCommand(ValidUserId, ValidEmployeeId, employeeDto);
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

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            employee.DepartmentId.Should().Be(UpdatedDepartmentId);
            employee.PersonalCode.Should().Be(command.Employee.PersonalCode);
            employee.NationalCode.Should().Be(command.Employee.NationalCode);
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
    public async Task Handle_WhenPersonalCodeIsDuplicate_ShouldNotCallUpdateAsync()
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
