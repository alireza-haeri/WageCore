namespace Application.Tests.Features.Employees.Command.TerminateEmployee;

public class TerminateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly TerminateEmployeeCommandHandler _handler;
    private readonly EmployeeBuilder _employeeBuilder;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
    private static readonly DateOnly ValidTerminationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

    public TerminateEmployeeCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _employeeBuilder = new EmployeeBuilder();
        _handler = new TerminateEmployeeCommandHandler(_employeeRepository);
    }

    private Employee CreateValidEmployee()
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

    private static TerminateEmployeeCommand CreateValidCommand(DateOnly? terminationDate = null) =>
        new(ValidUserId, ValidEmployeeId, terminationDate ?? ValidTerminationDate);

    [Fact]
    public async Task Handle_WithValidData_ShouldTerminateEmployeeAndReturnTrue()
    {
        var employee = CreateValidEmployee();
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            employee.TerminationDate.Should().Be(ValidTerminationDate);
            employee.IsTerminated.Should().BeTrue();
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
    public async Task Handle_WhenTerminationDateIsNull_ShouldReturnGeneralFailure()
    {
        var employee = CreateValidEmployee();
        var command = new TerminateEmployeeCommand(ValidUserId, ValidEmployeeId, null);

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ ترک کار نمیتواند خالی باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenEmployeeIsAlreadyTerminated_ShouldReturnGeneralFailure()
    {
        var employee = CreateValidEmployee();
        employee.Terminate(ValidTerminationDate).ShouldBeSuccess();
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("کارمند قبلاً ترک کار شده است.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenTerminationDateIsBeforeHireDate_ShouldReturnGeneralFailure()
    {
        var employee = CreateValidEmployee();
        var command = CreateValidCommand(ValidHireDate.AddDays(-1));

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ ترک کار نباید قبل از تاریخ استخدام باشد.", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var employee = CreateValidEmployee();
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsyncOnce()
    {
        var employee = CreateValidEmployee();
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _employeeRepository.UpdateAsync(employee, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.Received(1).GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAsyncOnce()
    {
        var employee = CreateValidEmployee();
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
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
    public async Task Handle_WhenDomainTerminateFails_ShouldNotCallUpdateAsync()
    {
        var employee = CreateValidEmployee();
        employee.Terminate(ValidTerminationDate).ShouldBeSuccess();
        var command = CreateValidCommand();

        _employeeRepository.GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }
}
