namespace Application.Tests.Features.Employees.Command.RehireEmployee;

public class RehireEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IWorkShopRepository _workshopRepository;
    private readonly RehireEmployeeCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;
    private readonly WorkshopBuilder _workshopBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private static readonly Guid RehireDepartmentId = Guid.NewGuid();
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-20));
    private static readonly DateOnly ValidTerminationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
    private static readonly DateOnly ValidRehireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

    public RehireEmployeeCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _workshopRepository = Substitute.For<IWorkShopRepository>();
        _employeeBuilder = new EmployeeBuilder();
        _workshopBuilder = new WorkshopBuilder();
        _handler = new RehireEmployeeCommandHandler(_employeeRepository, _workshopRepository);
    }

    private Employee CreateTerminatedEmployee()
    {
        var employee = _employeeBuilder
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithDepartmentId(ValidDepartmentId)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .WithHireDate(ValidHireDate)
            .CreateResult()
            .ShouldBeSuccess();

        employee.Terminate(ValidTerminationDate).ShouldBeSuccess();

        return employee;
    }

    private Employee CreateActiveEmployee()
    {
        return _employeeBuilder
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithDepartmentId(ValidDepartmentId)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .WithHireDate(ValidHireDate)
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

    private static RehireEmployeeCommand CreateValidCommand(Guid? departmentId = null, DateOnly? hireDate = null) =>
        new(ValidUserId, ValidEmployeeId, departmentId ?? RehireDepartmentId, hireDate ?? ValidRehireDate);

    [Fact]
    public async Task Handle_WithValidData_ShouldRehireEmployeeAndReturnTrue()
    {
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            employee.DepartmentId.Should().Be(RehireDepartmentId);
            employee.HireDate.Should().Be(ValidRehireDate);
            employee.TerminationDate.Should().BeNull();
            employee.IsTerminated.Should().BeFalse();
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
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand();

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
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDepartmentBelongsToAnotherWorkshop_ShouldReturnNotFoundFailure()
    {
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();
        var otherWorkshop = CreateValidWorkshop(Guid.NewGuid());

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(otherWorkshop);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenEmployeeIsNotTerminated_ShouldReturnGeneralFailure()
    {
        var employee = CreateActiveEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تنها کارمند ترک کار شده را میتوان دوباره استخدام کرد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenHireDateIsNotAfterTerminationDate_ShouldReturnGeneralFailure()
    {
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand(hireDate: ValidTerminationDate);
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ استخدام مجدد باید بعد از تاریخ ترک کار باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenHireDateIsBeforeWorkshopRegistrationDate_ShouldReturnGeneralFailure()
    {
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop(registrationDate: DateOnly.FromDateTime(DateTime.Now));

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ استخدام نباید قبل از تاریخ ثبت کارگاه باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenHireDateIsNull_ShouldReturnGeneralFailure()
    {
        var employee = CreateTerminatedEmployee();
        var command = new RehireEmployeeCommand(ValidUserId, ValidEmployeeId, RehireDepartmentId, null);
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ استخدام نمیتواند خالی باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncOnce()
    {
        var employee = CreateTerminatedEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.Received(1).UpdateAsync(employee, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldNotCallWorkshopRepository()
    {
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _workshopRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _employeeRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRehireFails_ShouldNotCallUpdateAsync()
    {
        var employee = CreateActiveEmployee();
        var command = CreateValidCommand();
        var workshop = CreateValidWorkshop();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _workshopRepository.GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(workshop);
        _workshopRepository.GetByDepartmentIdAsync(ValidUserId, RehireDepartmentId, Arg.Any<CancellationToken>())
            .Returns(workshop);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }
}
