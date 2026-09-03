using Application.Features.PayrollRecords;
using Core.Contracts.PayrollRecords;
using Core.Domain;
using Core.Domain.Enums;
using NSubstitute;
using Shared.Tests.Builders;

namespace Application.Tests.Features.PayrollRecords.Query.GetPayrollRecordCalculationDetails;

public class GetPayrollRecordCalculationDetailsQueryHandlerTests
{
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IWorkShopRepository _workShopRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollLimitsResolver _payrollLimitsResolver;
    private readonly ISalaryDecreeQuery _salaryDecreeQuery;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly IPayrollCalculationService _payrollCalculationService;
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
    private readonly Workshop _workshop;
    private readonly SalaryDecree _salaryDecree;

    public GetPayrollRecordCalculationDetailsQueryHandlerTests()
    {
        _employee = new EmployeeBuilder()
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithHireDate(HireDate)
            .CreateResult()
            .ShouldBeSuccess();
        _workshop = new WorkshopBuilder()
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
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
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _payrollLimitsResolver = Substitute.For<IPayrollLimitsResolver>();
        _salaryDecreeQuery = Substitute.For<ISalaryDecreeQuery>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _payrollCalculationService = Substitute.For<IPayrollCalculationService>();

        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(_employee);
        _workShopRepository
            .GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(_workshop);
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
        _payrollLimitsResolver
            .ResolveAsync(PeriodStart, PeriodEnd, Arg.Any<CancellationToken>())
            .Returns(Result<PayrollLimits>.Success(new PayrollLimits(20m, 12m, 8m, 8m)));
        _payrollRecordQuery
            .GetAnnualWorkedDaysCountAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                Arg.Any<CancellationToken>())
            .Returns(6m);
        SetupRuleValues();
        SetupCalculation(ValidCalculation());

        _handler = new GetPayrollRecordCalculationDetailsQueryHandler(
            _payrollRecordRepository,
            _employeeRepository,
            _workShopRepository,
            _persianCalendarService,
            _payrollLimitsResolver,
            _salaryDecreeQuery,
            _laborLawRuleQuery,
            _payrollRecordQuery,
            _payrollCalculationService);
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

    private void SetupRuleValues() =>
        _laborLawRuleQuery
            .GetActiveValueAsync(
                Arg.Any<LaborLawRuleKey>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<LaborLawRuleKey>() switch
            {
                LaborLawRuleKey.StandardDailyWorkHours => 7.33m,
                LaborLawRuleKey.OvertimePercentage => 140m,
                _ => (decimal?)null
            });

    private void SetupCalculation(PayrollCalculationResult calculation) =>
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<SalaryDecree>>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInput>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PayrollCalculationResult>.Success(calculation));

    private void SetupCalculationFailure(string message) =>
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<SalaryDecree>>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInput>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PayrollCalculationResult>.GeneralFailure(message));

    private static PayrollCalculationResult ValidCalculation() => new(
        new PayrollCalculatedAmountsDto(
            BaseSalaryAmount: 10_000_000m,
            AttractionAllowanceAmount: 1_000_000m,
            SupervisionAllowanceAmount: 500_000m,
            NightShiftExtraAmount: 300_000m,
            HolidayWorkAmount: 200_000m,
            ChildAllowanceAmount: 400_000m,
            HousingAllowanceAmount: 1_500_000m,
            FoodAllowanceAmount: 1_000_000m,
            MarriageAllowanceAmount: 1_200_000m,
            OvertimeAmount: 800_000m,
            ShiftWorkAmount: 900_000m,
            DailyMissionAmount: 20_000_000m,
            FridayWorkAllowance: 250_000m,
            EndOfServiceAmount: 300_000m,
            AnnualBonusAmount: null,
            CommutingAllowanceAmount: 700_000m,
            PerformanceBonusAmount: 1_000_000m,
            CashBenefitsAmount: 500_000m),
        new PayrollRecordAmountsDto(
            CalculatedTaxAmount: 1_500_000m,
            GrossAmount: 41_600_000m,
            InsuranceAmount: 2_500_000m,
            TotalDeductionsAmount: 4_000_000m,
            NetPayableAmount: 37_600_000m),
        false);

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnAllCalculationInputsAndResults()
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
            response.PreviousAnnualWorkedDaysCount.Should().Be(6m);
            response.AnnualWorkedDaysCount.Should().Be(30m);
            response.MaxMonthlyOvertimeHours.Should().Be(20m);
            response.MaxFridayHours.Should().Be(12m);
            response.MaxNightShiftHours.Should().Be(8m);
            response.DailyWorkingHours.Should().Be(8m);
            response.DecreeEffectiveFrom.Should().Be(DecreeEffectiveFrom);
            response.BaseDailySalary.Should().Be(_salaryDecree.BaseDailySalary);
            response.ChildrenCount.Should().Be(_salaryDecree.ChildrenCount);
            response.MaritalStatus.Should().Be(_salaryDecree.MaritalStatus);
            response.ShiftType.Should().Be(_salaryDecree.ShiftType);
            response.ContractType.Should().Be(_salaryDecree.ContractType);
            response.IsTaxSubject.Should().Be(_salaryDecree.IsTaxSubject);
            response.RuleValues.Should().BeEquivalentTo(
                [
                    new PayrollCalculationRuleValue(LaborLawRuleKey.StandardDailyWorkHours, 7.33m),
                    new PayrollCalculationRuleValue(LaborLawRuleKey.OvertimePercentage, 140m)
                ]);
            response.CalculatedAmounts.BaseSalaryAmount.Should().Be(10_000_000m);
            response.CalculatedAmounts.OvertimeAmount.Should().Be(800_000m);
            response.Amounts.GrossAmount.Should().Be(41_600_000m);
            response.Amounts.InsuranceAmount.Should().Be(2_500_000m);
            response.Amounts.CalculatedTaxAmount.Should().Be(1_500_000m);
            response.Amounts.NetPayableAmount.Should().Be(37_600_000m);
        }
    }

    [Fact]
    public async Task Handle_ShouldReconstructTheWorkInputFromThePersistedRecord()
    {
        SetupPayrollRecord(CreateValidPayrollRecord());

        await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        _payrollCalculationService.Received(1).CalculateAsync(
            _employee,
            _workshop,
            Arg.Any<IReadOnlyList<SalaryDecree>>(),
            PeriodStart,
            PeriodEnd,
            workInput => workInput.WorkedDaysCount == 24m &&
                         workInput.OvertimeHours == 4m &&
                         workInput.MissionDaysCount == 1m &&
                         workInput.StandardWorkingDaysCount == 31 &&
                         workInput.IsEsfandPeriod == false,
            Arg.Any<CancellationToken>());
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

        response.DecreeEffectiveFrom.Should().Be(new DateOnly(2025, 6, 15));
        response.BaseDailySalary.Should().Be(22_000_000m);
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

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotfoundFailure()
    {
        _workShopRepository
            .GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);
        SetupPayrollRecord(CreateValidPayrollRecord());

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("کارگاه مورد نظر یافت نشد.", BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoActiveSalaryDecree_ShouldReturnNotfoundFailure()
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
    public async Task Handle_WhenTheLimitsCannotBeResolved_ShouldReturnTheResolverErrors()
    {
        _payrollLimitsResolver
            .ResolveAsync(PeriodStart, PeriodEnd, Arg.Any<CancellationToken>())
            .Returns(Result<PayrollLimits>.NotfoundFailure("ساعات کار روزانه یافت نشد."));
        SetupPayrollRecord(CreateValidPayrollRecord());

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("ساعات کار روزانه یافت نشد.", BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenTheCalculationFails_ShouldReturnTheCalculationErrors()
    {
        SetupCalculationFailure("قانون اضافه‌کاری برای محاسبه مبلغ اضافه‌کاری یافت نشد.");
        SetupPayrollRecord(CreateValidPayrollRecord());

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeFailure("قانون اضافه‌کاری برای محاسبه مبلغ اضافه‌کاری یافت نشد.", BadResultType.Validation);
    }

    [Fact]
    public async Task Handle_WhenARuleValueIsMissing_ShouldOmitItFromTheRuleValues()
    {
        _laborLawRuleQuery
            .GetActiveValueAsync(
                Arg.Any<LaborLawRuleKey>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns((decimal?)null);
        SetupPayrollRecord(CreateValidPayrollRecord());

        var response = (await _handler.Handle(CreateValidQuery(), CancellationToken.None)).ShouldBeSuccess();

        response.RuleValues.Should().BeEmpty();
    }
}
