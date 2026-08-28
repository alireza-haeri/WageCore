namespace Core.Contracts.PayrollRecords;

public record PayrollRecordDto(
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    decimal? WorkedDaysCount,
    decimal? OvertimeHours,
    decimal? NightShiftHours,
    decimal? FridayWorkHours,
    decimal? LeaveDaysCount,
    decimal? AbsenceDaysCount,
    decimal? MissionDaysCount,
    decimal? OvertimeAmount,
    decimal? NightShiftExtraAmount,
    decimal? FridayWorkAllowance,
    decimal? CalculatedTaxAmount,
    decimal? NetPayableAmount);
