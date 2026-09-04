using Application.Features.PayrollRecords;
using Core.Domain;
using Core.Domain.Enums;
using NSubstitute;
using Shared.Tests.Builders;

namespace Application.Tests.Features.PayrollRecords.Query.GetPayrollRecordCalculationDetails;

public class GetPayrollRecordCalculationDetailsQueryHandlerTests
{
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly ISalaryDecreeQuery _salaryDecreeQuery;
    private readonly GetPayrollRecordCalculationDetailsQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = new(2025, 6, 20);
    private static readonly DateOnly PeriodEnd = new(2025, 7, 19);
    private static readonly DateOnly HireDate = new(2024, 1, 10);
    private static readonly DateOnly DecreeEffectiveFrom = new(2025, 6, 1);

    private readonly Employee _employee;
    private readonly SalaryDecree _salaryDecree;

    public GetPayrollRecordCalculationDetailsQueryHandlerTests()
    {
        _employee = new EmployeeBuilder()
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            // The builder defaults the workshop registration date to "now - 30 days",
            // which is after the fixed hire date below and would fail Employee.Create.
            .WithWorkshopRegistrationDate(new DateOnly(2024, 1, 1))
            .WithHireDate(HireDate)
            .CreateResult()
            .ShouldBeSuccess();
        _salaryDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(ValidEmployeeId)
            .WithEmployeeHireDate(HireDate)
            .WithEffectiveFrom(DecreeEffectiveFrom)
            .CreateResult()
            .ShouldBeSuccess();

        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _salaryDecreeQuery = Substitute.For<ISalaryDecreeQuery>();

        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(_employee);
        _salaryDecreeQuery
            .GetSalaryDecreesAffectingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                PeriodEnd,
                Arg.Any<CancellationToken>())
            .Returns(new List<SalaryDecree> { _salaryDecree });
        _persianCalendarService
            .GetPersianYear(PeriodStart)
            .Returns(1404);
        _persianCalendarService
            .GetPersianMonth(PeriodStart)
            .Returns(6);
        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodEnd)
            .Returns(5);
        _persianCalendarService
            .GetDaysInPersianYear(PeriodStart)
            .Returns(365);

        _handler = new GetPayrollRecordCalculationDetailsQueryHandler(
            _payrollRecordRepository,
            _employeeRepository,
            _persianCalendarService,
            _salaryDecreeQuery);
    }

    private static PayrollRecord CreateValidPayrollRecord() =>
        new PayrollRecordBuilder()
            .WithId(ValidPayrollRecordId)
            .WithEmployeeId(ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

    private GetPayrollRecordCalculationDetailsQuery CreateValidQuery() =>
        new(ValidUserId, ValidPayrollRecordId);

    private void SetupPayrollRecord(PayrollRecord? payrollRecord) =>
        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns(payrollRecord);

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnThePersistedRecordAndTheLiveDecree()
    {
        SetupPayrollRecord(CreateValidPayrollRecord());

        var response = (await _handler.Handle(CreateValidQuery(), CancellationToken.None)).ShouldBeSuccess();

        using (new FluentAssertions.Execution.AssertionScope())
        {
            response.PayrollRecordId.Should().Be(ValidPayrollRecordId);
            response.EmployeeId.Should().Be(ValidEmployeeId);
            response.EmployeeName.Should().Be(_employee.FullName);
            response.PersonalCode.Should().Be(_employee.PersonalCode);
            response.EmployeeHireDate.Should().Be(HireDate);
            response.Status.Should().Be(PayrollRecordStatus.Draft);
            response.PersianYear.Should().Be(1404);
            response.PersianMonth.Should().Be(6);
            response.PeriodStart.Should().Be(PeriodStart);
            response.PeriodEnd.Should().Be(PeriodEnd);
            response.PeriodDaysCount.Should().Be(30);
            response.FridayCount.Should().Be(5);
            response.DaysInYear.Should().Be(365);
            response.StandardWorkingDaysCount.Should().Be(31);
            response.WorkedDaysCount.Should().Be(24m);
            response.LeaveHours.Should().Be(2m);
            response.AbsenceDaysCount.Should().Be(0m);
            response.OvertimeHours.Should().Be(4m);
            response.NightShiftHours.Should().Be(3m);
            response.FridayWorkHours.Should().Be(2m);
            response.HolidayWorkHours.Should().Be(0m);
            response.MissionDaysCount.Should().Be(1m);
            response.MissionHours.Should().Be(0m);
            response.MissionAmountOverride.Should().BeNull();
            response.PerformanceBonusAmount.Should().BeNull();
            response.CashBenefitsAmount.Should().BeNull();
            response.AnnualBonusType.Should().BeNull();
            response.IsEsfandPeriod.Should().BeFalse();
            response.MaxMonthlyOvertimeHours.Should().Be(20m);
            response.MaxFridayHours.Should().Be(12m);
            response.MaxNightShiftHours.Should().Be(3m);
            response.DailyWorkingHours.Should().Be(8m);
            response.DecreeEffectiveFrom.Should().Be(DecreeEffectiveFrom);
            response.BaseDailySalary.Should().Be(_salaryDecree.BaseDailySalary);
            response.AttractionAllowance.Should().Be(_salaryDecree.AttractionAllowance);
            response.SupervisionAllowance.Should().Be(_salaryDecree.SupervisionAllowance);
            response.TransportationAllowanceNet.Should().Be(_salaryDecree.TransportationAllowanceNet);
            response.ChildrenCount.Should().Be(_salaryDecree.ChildrenCount);
            response.MaritalStatus.Should().Be(_salaryDecree.MaritalStatus);
            response.ShiftType.Should().Be(_salaryDecree.ShiftType);
            response.ContractType.Should().Be(_salaryDecree.ContractType);
            response.IsTaxSubject.Should().Be(_salaryDecree.IsTaxSubject);
            response.CalculatedAmounts.BaseSalaryAmount.Should().Be(10_000_000m);
            response.CalculatedAmounts.OvertimeAmount.Should().Be(800_000m);
            response.Amounts.GrossAmount.Should().Be(41_600_000m);
            response.Amounts.InsuranceAmount.Should().Be(2_500_000m);
            response.Amounts.CalculatedTaxAmount.Should().Be(1_500_000m);
            response.Amounts.NetPayableAmount.Should().Be(37_600_000m);
        }
    }

    [Fact]
    public async Task Handle_WithMultipleDecrees_ShouldUseTheLatestDecreeEffectiveByPeriodEnd()
    {
        var olderDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(ValidEmployeeId)
            .WithEmployeeHireDate(HireDate)
            .WithEffectiveFrom(new DateOnly(2025, 5, 1))
            .WithBaseDailySalary(18_000_000m)
            .CreateResult()
            .ShouldBeSuccess();
        var newerDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(ValidEmployeeId)
            .WithEmployeeHireDate(HireDate)
            .WithEffectiveFrom(new DateOnly(2025, 6, 15))
            .WithBaseDailySalary(22_000_000m)
            .CreateResult()
            .ShouldBeSuccess();
        _salaryDecreeQuery
            .GetSalaryDecreesAffectingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                PeriodEnd,
                Arg.Any<CancellationToken>())
            .Returns(new List<SalaryDecree> { olderDecree, newerDecree });
        SetupPayrollRecord(CreateValidPayrollRecord());

        var response = (await _handler.Handle(CreateValidQuery(), CancellationToken.None)).ShouldBeSuccess();

        using (new FluentAssertions.Execution.AssertionScope())
        {
            response.DecreeEffectiveFrom.Should().Be(new DateOnly(2025, 6, 15));
            response.BaseDailySalary.Should().Be(22_000_000m);
        }
    }

    [Fact]
    public async Task Handle_WhenNoDecreeIsEffectiveByPeriodEnd_ShouldReturnNotfoundFailure()
    {
        _salaryDecreeQuery
            .GetSalaryDecreesAffectingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                PeriodEnd,
                Arg.Any<CancellationToken>())
            .Returns(new List<SalaryDecree>());
        SetupPayrollRecord(CreateValidPayrollRecord());

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("حکم حقوقی فعال برای این کارمند در این بازه یافت نشد.", BadResultType.NotFound);
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
        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        SetupPayrollRecord(CreateValidPayrollRecord());

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
    }
}
