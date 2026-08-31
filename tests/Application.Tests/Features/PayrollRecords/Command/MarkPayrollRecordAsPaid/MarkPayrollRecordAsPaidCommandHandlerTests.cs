namespace Application.Tests.Features.PayrollRecords.Command.MarkPayrollRecordAsPaid;

public class MarkPayrollRecordAsPaidCommandHandlerTests
{
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly MarkPayrollRecordAsPaidCommandHandler _handler;

    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
    private static readonly DateOnly PeriodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

    public MarkPayrollRecordAsPaidCommandHandlerTests()
    {
        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();
        _handler = new MarkPayrollRecordAsPaidCommandHandler(_payrollRecordRepository);
    }

    private static MarkPayrollRecordAsPaidCommand CreateValidCommand(
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

    private Task DidNotReceiveUpdate() =>
        _payrollRecordRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());

    [Fact]
    public async Task Handle_WithDraftRecord_ShouldMarkItAsPaidAndReturnTrue()
    {
        var payrollRecord = CreateRecord();
        SetupFoundRecord(payrollRecord);
        _payrollRecordRepository
            .UpdateAsync(payrollRecord, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeSuccess().Should().BeTrue();
        payrollRecord.Status.Should().Be(PayrollRecordStatus.Paid);
        await _payrollRecordRepository.Received(1).UpdateAsync(
            Arg.Is<PayrollRecord>(x =>
                x == payrollRecord &&
                x.Id == ValidPayrollRecordId &&
                x.EmployeeId == ValidEmployeeId &&
                x.Status == PayrollRecordStatus.Paid),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordIsAlreadyPaid_ShouldReturnGeneralFailure()
    {
        SetupFoundRecord(CreateRecord(isPaid: true));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی قبلاً پرداخت شده است.", BadResultType.General);
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordNotFound_ShouldReturnNotfoundFailure()
    {
        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns((PayrollRecord?)null);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی مورد نظر یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordBelongsToAnotherEmployee_ShouldReturnNotfoundFailure()
    {
        SetupFoundRecord(CreateRecord(employeeId: Guid.NewGuid()));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی مورد نظر یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenRepositoryFailsToSave_ShouldReturnGeneralFailure()
    {
        var payrollRecord = CreateRecord();
        SetupFoundRecord(payrollRecord);
        _payrollRecordRepository
            .UpdateAsync(payrollRecord, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("خطا در بروزرسانی فیش پرداختی", BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldChangeTheStatusOnly()
    {
        var payrollRecord = CreateRecord();
        var workedDaysCount = payrollRecord.WorkedDaysCount;
        var overtimeHours = payrollRecord.OvertimeHours;
        var netPayableAmount = payrollRecord.NetPayableAmount;
        SetupFoundRecord(payrollRecord);
        _payrollRecordRepository
            .UpdateAsync(payrollRecord, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        using (new AssertionScope())
        {
            payrollRecord.WorkedDaysCount.Should().Be(workedDaysCount);
            payrollRecord.OvertimeHours.Should().Be(overtimeHours);
            payrollRecord.NetPayableAmount.Should().Be(netPayableAmount);
            payrollRecord.PeriodStart.Should().Be(PeriodStart);
            payrollRecord.PeriodEnd.Should().Be(PeriodEnd);
        }
    }
}
