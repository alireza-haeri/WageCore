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
        SetupRule(LaborLawRuleKey.MaximumOvertimeHoursPerDay, 4m);
        SetupRule(LaborLawRuleKey.MaximumNightShiftHoursPerDay, 3m);
        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodEnd)
            .Returns(4);
    }

    private void SetupRule(LaborLawRuleKey key, decimal? value)
    {
        var values = new Dictionary<LaborLawRuleKey, decimal>
        {
            [LaborLawRuleKey.StandardDailyWorkHours] = 7m,
            [LaborLawRuleKey.MaximumOvertimeHoursPerDay] = 4m,
            [LaborLawRuleKey.MaximumNightShiftHoursPerDay] = 3m
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
    public async Task ResolveAsync_ShouldCountTheDailyCeilingsOverEveryDayOfThePeriod()
    {
        var result = await Resolve();

        var limits = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            limits.MaxMonthlyOvertimeHours.Should().Be(112m);
            limits.MaxFridayHours.Should().Be(28m);
            limits.MaxNightShiftHours.Should().Be(84m);
        }
    }

    [Fact]
    public async Task ResolveAsync_WithASingleDayPeriod_ShouldCountThatDayOnce()
    {
        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodStart)
            .Returns(1);

        var result = await Resolve(periodEnd: PeriodStart);

        var limits = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            limits.MaxMonthlyOvertimeHours.Should().Be(4m);
            limits.MaxFridayHours.Should().Be(7m);
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
        SetupRule(LaborLawRuleKey.MaximumOvertimeHoursPerDay, null);

        var result = await Resolve();

        result.ShouldBeFailure("حداکثر ساعات اضافه‌کاری روزانه یافت نشد.", BadResultType.NotFound);
        await _laborLawRuleQuery.Received(1)
            .GetActiveRuleValuesAsync(PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_WhenAFridayWorkedDayIsZeroHours_ShouldAllowNoFridayHours()
    {
        SetupRule(LaborLawRuleKey.StandardDailyWorkHours, 0m);

        var result = await Resolve();

        var limits = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            limits.MaxFridayHours.Should().Be(0m);
            limits.MaxMonthlyOvertimeHours.Should().Be(112m);
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
    [InlineData(LaborLawRuleKey.MaximumOvertimeHoursPerDay, "حداکثر ساعات اضافه‌کاری روزانه یافت نشد.")]
    [InlineData(LaborLawRuleKey.StandardDailyWorkHours, "ساعات کار روزانه یافت نشد.")]
    [InlineData(LaborLawRuleKey.MaximumNightShiftHoursPerDay, "حداکثر ساعات شیفت شب روزانه یافت نشد.")]
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
