namespace Application.Services;

public class PayrollLimitsResolver(
    IPersianCalendarService persianCalendarService,
    ILaborLawRuleQuery laborLawRuleQuery)
    : IPayrollLimitsResolver
{
    public async Task<Result<PayrollLimits>> ResolveAsync(
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        // All rule values active at the period start, in a single query.
        var ruleValues = await laborLawRuleQuery.GetActiveRuleValuesAsync(
            periodStart,
            cancellationToken);

        if (!ruleValues.TryGetValue(LaborLawRuleKey.MaximumOvertimeHoursPerDay, out var maxOvertimeHoursPerDay))
            return Result<PayrollLimits>.NotfoundFailure("حداکثر ساعات اضافه‌کاری روزانه یافت نشد.");

        if (!ruleValues.TryGetValue(LaborLawRuleKey.StandardDailyWorkHours, out var dailyWorkingHours))
            return Result<PayrollLimits>.NotfoundFailure("ساعات کار روزانه یافت نشد.");

        if (!ruleValues.TryGetValue(LaborLawRuleKey.MaximumNightShiftHoursPerDay, out var maxNightShiftHoursPerDay))
            return Result<PayrollLimits>.NotfoundFailure("حداکثر ساعات شیفت شب روزانه یافت نشد.");

        var periodDaysCount = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var fridayDaysCount = persianCalendarService.GetFridayCount(periodStart, periodEnd);

        return Result<PayrollLimits>.Success(new PayrollLimits(
            maxOvertimeHoursPerDay * periodDaysCount,
            dailyWorkingHours * fridayDaysCount,
            maxNightShiftHoursPerDay * periodDaysCount,
            dailyWorkingHours));
    }
}
