namespace Application.Tests.Services;

public class PayrollLimitsResolverTests
{
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly PayrollLimitsResolver _resolver;

    private static readonly DateOnly PeriodStart = new(2025, 2, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 2, 28);

    public PayrollLimitsResolverTests()
    {
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _resolver = new PayrollLimitsResolver(_persianCalendarService, _laborLawRuleQuery);

        SetupRule(LaborLawRuleKey.StandardDailyWorkHours, 7m);
        SetupRule(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, 100m);
        SetupRule(LaborLawRuleKey.NightShiftHoursPerDay, 3m);
        SetupRule(LaborLawRuleKey.FridayWorkHoursPerDay, 16m);
        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodEnd)
            .Returns(4);
    }

    private void SetupRule(LaborLawRuleKey key, decimal? value)
    {
        var values = new Dictionary<LaborLawRuleKey, decimal>
        {
            [LaborLawRuleKey.StandardDailyWorkHours] = 7m,
            [LaborLawRuleKey.MaximumOvertimeHoursPerMonth] = 100m,
            [LaborLawRuleKey.NightShiftHoursPerDay] = 3m,
            [LaborLawRuleKey.FridayWorkHoursPerDay] = 16m
        };
        if (value is null)
            values.Remove(key);
        else
            values[key] = value.Value;

        _laborLawRuleQuery
            .GetActiveRuleValuesAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyDictionary<LaborLawRuleKey, decimal>>(values);
    }

    private Task<Result<PayrollLimits>> Resolve(DateOnly? periodEnd = null) =>
        _resolver.ResolveAsync(PeriodStart, periodEnd ?? PeriodEnd, CancellationToken.None);

    [Fact]
    public async Task ResolveAsync_ShouldUseTheMonthlyOvertimeRuleAndMultiplyTheDailyRulesOverThePeriod()
    {
        var result = await Resolve();

        var limits = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            limits.MaxMonthlyOvertimeHours.Should().Be(100m);
            limits.MaxFridayHours.Should().Be(64m);
            limits.MaxNightShiftHours.Should().Be(84m);
            limits.DailyWorkingHours.Should().Be(7m);
        }
    }

    [Fact]
    public async Task ResolveAsync_WithASingleDayPeriod_ShouldMultiplyTheDailyRulesByOneDay()
    {
        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodStart)
            .Returns(1);

        var result = await Resolve(periodEnd: PeriodStart);

        var limits = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            limits.MaxMonthlyOvertimeHours.Should().Be(100m);
            limits.MaxFridayHours.Should().Be(16m);
            limits.MaxNightShiftHours.Should().Be(3m);
        }
    }

    [Fact]
    public async Task ResolveAsync_ShouldAskTheCalendarForTheSamePeriodItWasGiven()
    {
        await Resolve();

        _persianCalendarService.Received(1).GetFridayCount(PeriodStart, PeriodEnd);
    }

    [Fact]
    public async Task ResolveAsync_ShouldReadTheRulesActiveAtThePeriodStart()
    {
        await Resolve();

        await _laborLawRuleQuery.Received(1)
            .GetActiveRuleValuesAsync(PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_ShouldLoadTheRulesInASingleQuery()
    {
        SetupRule(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, null);

        var result = await Resolve();

        result.ShouldBeFailure("حداکثر ساعات اضافه‌کاری ماهانه یافت نشد.", BadResultType.NotFound);
        await _laborLawRuleQuery.Received(1)
            .GetActiveRuleValuesAsync(PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_WhenAFridayWorkedDayIsZeroHours_ShouldAllowNoFridayHours()
    {
        SetupRule(LaborLawRuleKey.FridayWorkHoursPerDay, 0m);

        var result = await Resolve();

        var limits = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            limits.MaxFridayHours.Should().Be(0m);
            limits.MaxMonthlyOvertimeHours.Should().Be(100m);
        }
    }

    [Fact]
    public async Task ResolveAsync_WithoutAnyFridayInThePeriod_ShouldAllowNoFridayHours()
    {
        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodEnd)
            .Returns(0);

        var result = await Resolve();

        var limits = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            limits.MaxFridayHours.Should().Be(0m);
            limits.MaxNightShiftHours.Should().Be(84m);
        }
    }

    [Theory]
    [InlineData(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, "حداکثر ساعات اضافه‌کاری ماهانه یافت نشد.")]
    [InlineData(LaborLawRuleKey.FridayWorkHoursPerDay, "ساعات کار روز جمعه یافت نشد.")]
    [InlineData(LaborLawRuleKey.NightShiftHoursPerDay, "ساعات شیفت شب در روز یافت نشد.")]
    [InlineData(LaborLawRuleKey.StandardDailyWorkHours, "ساعات کار روزانه یافت نشد.")]
    public async Task ResolveAsync_WhenARuleIsNotConfigured_ShouldReturnNotfoundFailure(
        LaborLawRuleKey missingKey,
        string expectedMessage)
    {
        SetupRule(missingKey, null);

        var result = await Resolve();

        result.ShouldBeFailure(expectedMessage, BadResultType.NotFound);
        _persianCalendarService.DidNotReceive().GetFridayCount(Arg.Any<DateOnly>(), Arg.Any<DateOnly>());
    }
}
