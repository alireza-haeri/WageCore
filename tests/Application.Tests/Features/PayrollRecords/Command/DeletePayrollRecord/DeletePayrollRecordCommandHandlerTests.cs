namespace Application.Tests.Features.PayrollRecords.Command.DeletePayrollRecord;

public class DeletePayrollRecordCommandHandlerTests
{
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly DeletePayrollRecordCommandHandler _handler;

    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
    private static readonly DateOnly PeriodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

    public DeletePayrollRecordCommandHandlerTests()
    {
        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();
        _handler = new DeletePayrollRecordCommandHandler(_payrollRecordRepository);
    }

    private static DeletePayrollRecordCommand CreateValidCommand(
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? payrollRecordId = null) =>
        new(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            payrollRecordId ?? ValidPayrollRecordId);

    private PayrollRecord CreateRecord(Guid? employeeId = null, bool isPaid = false)
    {
        var payrollRecord = _payrollRecordBuilder
            .WithId(ValidPayrollRecordId)
            .WithEmployeeId(employeeId ?? ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

        if (isPaid)
            payrollRecord.MarkAsPaid().ShouldBeSuccess();

        return payrollRecord;
    }

    private void SetupFoundRecord(PayrollRecord payrollRecord) =>
        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns(payrollRecord);

    private Task DidNotReceiveDelete() =>
        _payrollRecordRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());

    [Fact]
    public async Task Handle_WithDraftRecord_ShouldDeleteItAndReturnTrue()
    {
        SetupFoundRecord(CreateRecord());
        _payrollRecordRepository
            .DeleteAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeSuccess().Should().BeTrue();
        await _payrollRecordRepository.Received(1).DeleteAsync(
            ValidUserId,
            ValidPayrollRecordId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordNotFound_ShouldReturnNotfoundFailure()
    {
        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns((PayrollRecord?)null);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی مورد نظر یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveDelete();
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordBelongsToAnotherEmployee_ShouldReturnNotfoundFailure()
    {
        SetupFoundRecord(CreateRecord(employeeId: Guid.NewGuid()));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی مورد نظر یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveDelete();
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordIsPaid_ShouldReturnGeneralFailure()
    {
        SetupFoundRecord(CreateRecord(isPaid: true));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی پرداخت شده قابل حذف نیست.", BadResultType.General);
        await DidNotReceiveDelete();
    }

    [Fact]
    public async Task Handle_WhenRepositoryFailsToDelete_ShouldReturnGeneralFailure()
    {
        SetupFoundRecord(CreateRecord());
        _payrollRecordRepository
            .DeleteAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("خطا در حذف فیش پرداختی", BadResultType.General);
    }
}
