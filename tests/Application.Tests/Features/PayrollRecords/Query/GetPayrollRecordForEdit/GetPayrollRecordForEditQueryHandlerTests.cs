using Core.Contracts.PayrollRecords;
using NSubstitute;
using Shared.Kernel.Common;
using Shared.Tests.Builders;

namespace Application.Tests.Features.PayrollRecords.Query.GetPayrollRecordForEdit;

public class GetPayrollRecordForEditQueryHandlerTests
{
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollLimitsResolver _payrollLimitsResolver;
    private readonly GetPayrollRecordForEditQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = new(2025, 6, 20);
    private static readonly DateOnly PeriodEnd = new(2025, 7, 19);

    private readonly Employee _employee;

    public GetPayrollRecordForEditQueryHandlerTests()
    {
        _employee = new EmployeeBuilder()
            .WithId(ValidEmployeeId)
            .CreateResult()
            .ShouldBeSuccess();

        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _payrollLimitsResolver = Substitute.For<IPayrollLimitsResolver>();

        _persianCalendarService
            .GetPersianYear(PeriodStart)
            .Returns(1404);
        _persianCalendarService
            .GetPersianMonth(PeriodStart)
            .Returns(6);
        _payrollLimitsResolver
            .ResolveAsync(PeriodStart, PeriodEnd, Arg.Any<CancellationToken>())
            .Returns(Result<PayrollLimits>.Success(new PayrollLimits(20m, 12m, 8m, 8m)));

        _handler = new GetPayrollRecordForEditQueryHandler(
            _payrollRecordRepository,
            _employeeRepository,
            _persianCalendarService,
            _payrollLimitsResolver);
    }

    private static PayrollRecord CreateValidPayrollRecord() =>
        new PayrollRecordBuilder()
            .WithId(ValidPayrollRecordId)
            .WithEmployeeId(ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

    private GetPayrollRecordForEditQuery CreateValidQuery() =>
        new(ValidUserId, ValidPayrollRecordId);

    private void SetupPayrollRecord(PayrollRecord? payrollRecord) =>
        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns(payrollRecord);

    private void SetupEmployee(Employee? employee) =>
        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnTheRecordWithItsStructuredWorkInput()
    {
        var payrollRecord = CreateValidPayrollRecord();
        SetupPayrollRecord(payrollRecord);
        SetupEmployee(_employee);

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        var response = result.ShouldBeSuccess();
        using (new FluentAssertions.Execution.AssertionScope())
        {
            response.PayrollRecordId.Should().Be(ValidPayrollRecordId);
            response.EmployeeId.Should().Be(ValidEmployeeId);
            response.EmployeeName.Should().Be(_employee.FullName);
            response.PersonalCode.Should().Be(_employee.PersonalCode);
            response.PersianYear.Should().Be(1404);
            response.PersianMonth.Should().Be(6);
            response.Status.Should().Be(PayrollRecordStatus.Draft);
            response.Work.WorkedDaysCount.Should().Be(24m);
            response.Work.Overtime.Should().Be(new WorkTimeInput(4, 0));
            response.Work.NightShift.Should().Be(new WorkTimeInput(3, 0));
            response.Work.FridayWork.Should().Be(new WorkTimeInput(2, 0));
            response.Work.HolidayWork.Should().Be(new WorkTimeInput(0, 0));
            response.Work.Leave.Should().Be(new DayTimeInput(0, 2, 0));
            response.Work.AbsenceDaysCount.Should().Be(0m);
            response.Work.MissionDays.Should().Be(1);
            response.Work.MissionHours.Should().Be(new WorkTimeInput(0, 0));
            response.OvertimeAmount.Should().Be(800_000m);
            response.NightShiftExtraAmount.Should().Be(300_000m);
            response.FridayWorkAllowance.Should().Be(250_000m);
            response.Amounts.CalculatedTaxAmount.Should().Be(1_500_000m);
            response.Amounts.GrossAmount.Should().Be(17_900_000m);
            response.Amounts.InsuranceAmount.Should().Be(1_400_000m);
            response.Amounts.TotalDeductionsAmount.Should().Be(2_900_000m);
            response.Amounts.NetPayableAmount.Should().Be(15_000_000m);
        }
    }

    [Fact]
    public async Task Handle_ShouldSplitTheLeaveHoursWithTheDailyWorkingHoursRule()
    {
        var payrollRecord = new PayrollRecordBuilder()
            .WithId(ValidPayrollRecordId)
            .WithEmployeeId(ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .WithLeaveHours(19m)
            .CreateResult()
            .ShouldBeSuccess();
        SetupPayrollRecord(payrollRecord);
        SetupEmployee(_employee);

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        // 19 hours with an 8-hour working day = 2 days and 3 hours.
        result.ShouldBeSuccess().Work.Leave.Should().Be(new DayTimeInput(2, 3, 0));
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordNotFound_ShouldReturnNotfoundFailure()
    {
        SetupPayrollRecord(null);

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی مورد نظر یافت نشد.", BadResultType.NotFound);
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotfoundFailure()
    {
        SetupPayrollRecord(CreateValidPayrollRecord());
        SetupEmployee(null);

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenTheLimitsCannotBeResolved_ShouldReturnTheResolverErrors()
    {
        SetupPayrollRecord(CreateValidPayrollRecord());
        SetupEmployee(_employee);
        _payrollLimitsResolver
            .ResolveAsync(PeriodStart, PeriodEnd, Arg.Any<CancellationToken>())
            .Returns(Result<PayrollLimits>.NotfoundFailure("ساعات کار روزانه یافت نشد."));

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("ساعات کار روزانه یافت نشد.", BadResultType.Validation);
    }
}
