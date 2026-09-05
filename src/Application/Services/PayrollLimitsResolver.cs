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

        // Monthly overtime cap: a direct monthly value from the labor law.
        if (!ruleValues.TryGetValue(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, out var maxOvertimeHoursPerMonth))
            return Result<PayrollLimits>.NotfoundFailure("حداکثر ساعات اضافه‌کاری ماهانه یافت نشد.");

        // Hours counted as Friday work on a single Friday.
        if (!ruleValues.TryGetValue(LaborLawRuleKey.FridayWorkHoursPerDay, out var fridayWorkHoursPerDay))
            return Result<PayrollLimits>.NotfoundFailure("ساعات کار روز جمعه یافت نشد.");

        // Hours counted as night shift work on a single day.
        if (!ruleValues.TryGetValue(LaborLawRuleKey.NightShiftHoursPerDay, out var nightShiftHoursPerDay))
            return Result<PayrollLimits>.NotfoundFailure("ساعات شیفت شب در روز یافت نشد.");

        if (!ruleValues.TryGetValue(LaborLawRuleKey.StandardDailyWorkHours, out var dailyWorkingHours))
            return Result<PayrollLimits>.NotfoundFailure("ساعات کار روزانه یافت نشد.");

        var periodDaysCount = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var fridayDaysCount = persianCalendarService.GetFridayCount(periodStart, periodEnd);

        return Result<PayrollLimits>.Success(new PayrollLimits(
            maxOvertimeHoursPerMonth,
            fridayWorkHoursPerDay * fridayDaysCount,
            nightShiftHoursPerDay * periodDaysCount,
            dailyWorkingHours));
    }
}
