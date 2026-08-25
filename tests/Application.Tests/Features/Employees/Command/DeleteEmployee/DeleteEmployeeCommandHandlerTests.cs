namespace Application.Tests.Features.Employees.Command.DeleteEmployee;

public class DeleteEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly DeleteEmployeeCommandHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    public DeleteEmployeeCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _handler = new DeleteEmployeeCommandHandler(_employeeRepository);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldDeleteEmployeeAndReturnTrue()
    {
        var command = new DeleteEmployeeCommand(ValidUserId, ValidEmployeeId);

        _employeeRepository.DeleteAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRepositoryDeleteFails_ShouldReturnGeneralFailure()
    {
        var command = new DeleteEmployeeCommand(ValidUserId, ValidEmployeeId);

        _employeeRepository.DeleteAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteAsyncOnce()
    {
        var command = new DeleteEmployeeCommand(ValidUserId, ValidEmployeeId);

        _employeeRepository.DeleteAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.Received(1).DeleteAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToRepository()
    {
        var command = new DeleteEmployeeCommand(ValidUserId, ValidEmployeeId);

        _employeeRepository.DeleteAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _employeeRepository.Received(1).DeleteAsync(
            ValidUserId,
            ValidEmployeeId,
            Arg.Any<CancellationToken>());
    }
}
