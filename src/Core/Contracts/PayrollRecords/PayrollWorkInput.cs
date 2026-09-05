namespace Core.Contracts.PayrollRecords;

public record PayrollWorkInput(
    int WorkedDaysCount,
    decimal OvertimeHours,
    decimal NightShiftHours,
    decimal FridayWorkHours,
    decimal LeaveHours,
    decimal MissionDaysCount,
    decimal MissionHours,
    decimal HolidayWorkHours,
    int HolidaysCount,
    decimal? MissionAmountOverride,
    int StandardWorkingDaysCount,
    bool IsEsfandPeriod,
    decimal? PerformanceBonusAmount,
    decimal? CashBenefitsAmount,
    AnnualBonusType? AnnualBonusType
);