namespace Core.Contracts.PayrollRecords;

/// <summary>
/// Attendance figures are entered as input, while the amount fields (overtime, night shift, Friday
/// work, tax and net payable) are produced by the payroll calculation and only stored on the record.
/// </summary>
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
