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
            AbsenceDaysCount: work.AbsenceDaysCount,
            MissionDaysCount: work.MissionDays,
            MissionHours: ToHours(work.MissionHours),
            HolidayWorkHours: ToHours(work.HolidayWork),
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
}
