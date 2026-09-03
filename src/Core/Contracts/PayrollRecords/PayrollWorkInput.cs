namespace Core.Contracts.PayrollRecords;

public record PayrollWorkInput(
    decimal WorkedDaysCount,
    decimal OvertimeHours,
    decimal NightShiftHours,
    decimal FridayWorkHours,
    decimal LeaveHours,
    decimal AbsenceDaysCount,
    decimal MissionDaysCount,
    decimal MissionHours,
    decimal HolidayWorkHours,
    decimal? MissionAmountOverride,
    int StandardWorkingDaysCount,
    bool IsEsfandPeriod,
    decimal? PerformanceBonusAmount,
    decimal? CashBenefitsAmount,
    AnnualBonusType? AnnualBonusType
);