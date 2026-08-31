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
        var maxOvertimeHoursPerDay = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MaximumOvertimeHoursPerDay,
            periodStart,
            cancellationToken);
        if (maxOvertimeHoursPerDay is null)
            return Result<PayrollLimits>.NotfoundFailure("حداکثر ساعات اضافه‌کاری روزانه یافت نشد.");

        var dailyWorkingHours = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.DailyWorkingHours,
            periodStart,
            cancellationToken);
        if (dailyWorkingHours is null)
            return Result<PayrollLimits>.NotfoundFailure("ساعات کار روزانه یافت نشد.");

        var maxNightShiftHoursPerDay = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MaximumNightShiftHoursPerDay,
            periodStart,
            cancellationToken);
        if (maxNightShiftHoursPerDay is null)
            return Result<PayrollLimits>.NotfoundFailure("حداکثر ساعات شیفت شب روزانه یافت نشد.");

        var periodDaysCount = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var fridayDaysCount = persianCalendarService.GetFridayCount(periodStart, periodEnd);

        return Result<PayrollLimits>.Success(new PayrollLimits(
            maxOvertimeHoursPerDay.Value * periodDaysCount,
            dailyWorkingHours.Value * fridayDaysCount,
            maxNightShiftHoursPerDay.Value * periodDaysCount));
    }
}
