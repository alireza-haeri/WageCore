using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Services;

public class PayrollCalculationServiceTests
{
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly PayrollCalculationService _service;

    private static readonly Employee TaxSubjectEmployee = new EmployeeBuilder()
        .WithChildrenCount(0)
        .WithIsTaxSubject(true)
        .CreateResult()
        .ShouldBeSuccess();
    private static readonly Workshop Workshop = new WorkshopBuilder().CreateResult().ShouldBeSuccess();
    private static readonly DateOnly PeriodStart = new(2025, 2, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 2, 24);
    private static readonly Guid EmployeeId = Guid.NewGuid();

    public PayrollCalculationServiceTests()
    {
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _service = new PayrollCalculationService(
            _laborLawRuleQuery,
            new Logger<PayrollCalculationService>(NullLoggerFactory.Instance));
        SetupDefaultRules();
    }

    private static PayrollWorkInputDto CreateWork(
        decimal workedDays = 24m,
        decimal overtimeHours = 4m,
        decimal nightShiftHours = 3m,
        decimal fridayWorkHours = 2m,
        decimal leaveDays = 0m,
        decimal absenceDays = 0m,
        decimal missionDays = 0m) =>
        new(
            workedDays,
            overtimeHours,
            nightShiftHours,
            fridayWorkHours,
            leaveDays,
            absenceDays,
            missionDays);

    private EmployeeSalaryProfile CreateSalaryProfile(
        decimal baseMonthlySalary = 20_000_000m,
        DateOnly? effectiveFrom = null) =>
        new EmployeeSalaryProfileBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 1, 1))
            .WithMinimumMonthlySalary(10_000_000m)
            .WithEffectiveFrom(effectiveFrom ?? new DateOnly(2025, 1, 1))
            .WithBaseMonthlySalary(baseMonthlySalary)
            .CreateResult()
            .ShouldBeSuccess();

    private void SetupRule(LaborLawRuleKey key, decimal? value) =>
        _laborLawRuleQuery
            .GetActiveValueAsync(key, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(value);

    private void SetupDefaultRules()
    {
        SetupRule(LaborLawRuleKey.MonthlyWorkingHours, 200m);
        SetupRule(LaborLawRuleKey.OvertimePremiumPercent, 35m);
        SetupRule(LaborLawRuleKey.NightShiftExtraPercent, 40m);
        SetupRule(LaborLawRuleKey.FridayWorkPercent, 40m);
        SetupRule(LaborLawRuleKey.TaxPercent, 10m);
        SetupRule(LaborLawRuleKey.MaximumMonthlyOvertimeHours, 20m);
        SetupRule(LaborLawRuleKey.MaximumFridayWorkHours, 12m);
    }

    private Task<Result<PayrollCalculationResult>> Calculate(
        PayrollWorkInputDto? work = null,
        EmployeeSalaryProfile[]? salaryProfiles = null,
        Employee? employee = null) =>
        _service.CalculateAsync(
            employee ?? TaxSubjectEmployee,
            Workshop,
            salaryProfiles ?? [CreateSalaryProfile()],
            PeriodStart,
            PeriodEnd,
            work ?? CreateWork(),
            CancellationToken.None);

    [Fact]
    public async Task CalculateAsync_WithAFullPeriodOfWork_ShouldPriceEveryComponentFromTheRules()
    {
        var result = await Calculate();

        var calculation = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            calculation.OvertimeAmount.Should().Be(540_000m);
            calculation.NightShiftExtraAmount.Should().Be(120_000m);
            calculation.FridayWorkAllowance.Should().Be(80_000m);
            calculation.CalculatedTaxAmount.Should().Be(2_074_000m);
            calculation.NetPayableAmount.Should().Be(18_666_000m);
            calculation.MaxMonthlyOvertimeHours.Should().Be(20m);
            calculation.MaxFridayHours.Should().Be(12m);
        }
    }

    [Fact]
    public async Task CalculateAsync_WhenPartOfThePeriodIsWorked_ShouldProrateTheMonthlyPackage()
    {
        var result = await Calculate(CreateWork(workedDays: 12m));

        var calculation = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            calculation.OvertimeAmount.Should().Be(540_000m);
            calculation.CalculatedTaxAmount.Should().Be(1_074_000m);
            calculation.NetPayableAmount.Should().Be(9_666_000m);
        }
    }

    [Theory]
    [InlineData(24m, 0m, 0m)]
    [InlineData(22m, 2m, 0m)]
    [InlineData(20m, 0m, 4m)]
    public async Task CalculateAsync_ShouldProrateTheFixedAllowancesLikeTheBaseSalary(
        decimal workedDays,
        decimal leaveDays,
        decimal missionDays)
    {
        var salaryProfile = new EmployeeSalaryProfileBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 1, 1))
            .WithMinimumMonthlySalary(10_000_000m)
            .WithEffectiveFrom(new DateOnly(2025, 1, 1))
            .WithBaseMonthlySalary(20_000_000m)
            .WithHousingAllowance(500_000m)
            .WithFoodAllowance(400_000m)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await Calculate(
            CreateWork(workedDays: workedDays, leaveDays: leaveDays, missionDays: missionDays),
            [salaryProfile]);

        var calculation = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            calculation.CalculatedTaxAmount.Should().Be(2_164_000m);
            calculation.NetPayableAmount.Should().Be(19_476_000m);
        }
    }

    [Fact]
    public async Task CalculateAsync_ShouldPayMissionAndLeaveDaysLikeWorkedDays()
    {
        var fullMonth = await Calculate();
        var result = await Calculate(CreateWork(workedDays: 20m, leaveDays: 2m, missionDays: 2m));

        result.ShouldBeSuccess()
            .NetPayableAmount
            .Should()
            .Be(fullMonth.ShouldBeSuccess().NetPayableAmount);
    }

    [Fact]
    public async Task CalculateAsync_ShouldNotPayAbsenceDays()
    {
        var result = await Calculate(CreateWork(workedDays: 20m, absenceDays: 4m));

        var calculation = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            calculation.OvertimeAmount.Should().Be(540_000m);
            calculation.CalculatedTaxAmount.Should().Be(1_740_667m);
            calculation.NetPayableAmount.Should().Be(15_666_000m);
        }
    }

    [Fact]
    public async Task CalculateAsync_WhenEmployeeIsNotTaxSubject_ShouldReturnZeroTax()
    {
        var employee = new EmployeeBuilder()
            .WithIsTaxSubject(false)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await Calculate(employee: employee);

        var calculation = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            calculation.CalculatedTaxAmount.Should().Be(0m);
            calculation.NetPayableAmount.Should().Be(20_740_000m);
        }
    }

    [Fact]
    public async Task CalculateAsync_WithMissingWorkCounts_ShouldTreatThemAsZero()
    {
        var result = await Calculate(new PayrollWorkInputDto(null, null, null, null, null, null, null));

        var calculation = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            calculation.OvertimeAmount.Should().Be(0m);
            calculation.NightShiftExtraAmount.Should().Be(0m);
            calculation.FridayWorkAllowance.Should().Be(0m);
            calculation.NetPayableAmount.Should().Be(0m);
        }
    }

    [Fact]
    public async Task CalculateAsync_WithNegativeHours_ShouldNotReduceThePay()
    {
        var result = await Calculate(CreateWork(overtimeHours: -5m));

        result.ShouldBeSuccess().OvertimeAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_ShouldUseTheProfileInForceAtThePeriodStart()
    {
        var older = CreateSalaryProfile(20_000_000m, new DateOnly(2025, 1, 1));
        var newer = CreateSalaryProfile(40_000_000m, new DateOnly(2025, 2, 10));

        var result = await Calculate(salaryProfiles: [newer, older]);

        result.ShouldBeSuccess().NetPayableAmount.Should().Be(18_666_000m);
    }

    [Fact]
    public async Task CalculateAsync_WhenNoProfileCoversThePeriod_ShouldReturnNotfoundFailure()
    {
        var result = await Calculate(salaryProfiles: []);

        result.ShouldBeFailure("برای این بازه حکم حقوقی کارمند یافت نشد.", BadResultType.NotFound);
    }

    [Theory]
    [InlineData(LaborLawRuleKey.MonthlyWorkingHours, "ساعات کار ماهانه یافت نشد.")]
    [InlineData(LaborLawRuleKey.OvertimePremiumPercent, "درصد اضافه‌کاری یافت نشد.")]
    [InlineData(LaborLawRuleKey.NightShiftExtraPercent, "درصد فوق‌العاده شیفت شب یافت نشد.")]
    [InlineData(LaborLawRuleKey.FridayWorkPercent, "درصد حق کار جمعه یافت نشد.")]
    [InlineData(LaborLawRuleKey.TaxPercent, "نرخ مالیات یافت نشد.")]
    [InlineData(LaborLawRuleKey.MaximumMonthlyOvertimeHours, "حداکثر ساعات اضافه‌کاری ماهانه یافت نشد.")]
    [InlineData(LaborLawRuleKey.MaximumFridayWorkHours, "حداکثر ساعات کار جمعه یافت نشد.")]
    public async Task CalculateAsync_WhenARuleIsNotConfigured_ShouldReturnNotfoundFailure(
        LaborLawRuleKey missingKey,
        string expectedMessage)
    {
        SetupRule(missingKey, null);

        var result = await Calculate();

        result.ShouldBeFailure(expectedMessage, BadResultType.NotFound);
    }

    [Fact]
    public async Task CalculateAsync_WhenMonthlyWorkingHoursIsZero_ShouldReturnNotfoundFailure()
    {
        SetupRule(LaborLawRuleKey.MonthlyWorkingHours, 0m);

        var result = await Calculate();

        result.ShouldBeFailure("ساعات کار ماهانه یافت نشد.", BadResultType.NotFound);
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(
                LaborLawRuleKey.OvertimePremiumPercent,
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_ShouldReadTheRulesActiveAtThePeriodStart()
    {
        await Calculate();

        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxPercent, PeriodStart, Arg.Any<CancellationToken>());
    }
}
