namespace Application.Features.PayrollRecords;

/// <summary>
/// Maps the structured user work input to the decimal-based <see cref="PayrollWorkInput"/>
/// consumed by the payroll domain.
/// Minutes are converted to hours with minutes / 60. Days are converted to hours with the
/// labor-law daily working hours rule resolved for the period, so the conversion always
/// reflects the rule that was in effect for that period.
/// </summary>
public static class PayrollWorkInputMapper
{
    public static PayrollWorkInput Map(
        UserWorkInputDto work,
        int standardWorkingDaysCount,
        bool isEsfandPeriod,
        decimal dailyWorkingHours) =>
        new(
            WorkedDaysCount: work.WorkedDaysCount,
            OvertimeHours: ToHours(work.Overtime),
            NightShiftHours: ToHours(work.NightShift),
            FridayWorkHours: ToHours(work.FridayWork),
            LeaveHours: ToHours(work.Leave, dailyWorkingHours),
            MissionDaysCount: work.MissionDays,
            MissionHours: ToHours(work.MissionHours),
            HolidayWorkHours: ToHours(work.HolidayWork),
            HolidaysCount: work.HolidaysCount,
            MissionAmountOverride: work.MissionAmountOverride,
            StandardWorkingDaysCount: standardWorkingDaysCount,
            IsEsfandPeriod: isEsfandPeriod,
            PerformanceBonusAmount: work.PerformanceBonusAmount,
            CashBenefitsAmount: work.CashBenefitsAmount,
            AnnualBonusType: work.AnnualBonusType);

    public static decimal ToHours(WorkTimeInput workTime) =>
        workTime.Hours + workTime.Minutes / 60m;

    public static decimal ToHours(DayTimeInput dayTime, decimal dailyWorkingHours) =>
        dayTime.Days * dailyWorkingHours + dayTime.Hours + dayTime.Minutes / 60m;

    /// <summary>
    /// Splits a decimal hours value back into hours and minutes, as the user would
    /// have entered it. Values that were entered as hours+minutes round-trip exactly.
    /// </summary>
    public static WorkTimeInput FromHours(decimal hours)
    {
        var wholeHours = (int)hours;
        var minutes = (int)Math.Round((hours - wholeHours) * 60m, MidpointRounding.AwayFromZero);

        if (minutes == 60)
        {
            wholeHours++;
            minutes = 0;
        }

        return new WorkTimeInput(wholeHours, minutes);
    }

    /// <summary>
    /// Splits a decimal hours value back into days, hours and minutes. The day part is
    /// derived with the labor-law daily working hours rule effective for the period, so
    /// a value that was entered as days+hours+minutes round-trips exactly.
    /// </summary>
    public static DayTimeInput FromHours(decimal hours, decimal dailyWorkingHours)
    {
        if (dailyWorkingHours <= 0)
            return new DayTimeInput(0, FromHours(hours).Hours, FromHours(hours).Minutes);

        var days = (int)Math.Floor(hours / dailyWorkingHours);
        var remainder = hours - days * dailyWorkingHours;
        var workTime = FromHours(remainder);
        return new DayTimeInput(days, workTime.Hours, workTime.Minutes);
    }
}
