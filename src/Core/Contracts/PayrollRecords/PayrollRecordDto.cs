namespace Core.Contracts.PayrollRecords;

public record PayrollRecordDto(
    decimal? WorkedDaysCount,
    decimal? OvertimeHours,
    decimal? NightShiftHours,
    decimal? FridayWorkHours,
    decimal? LeaveDaysCount,
    decimal? AbsenceDaysCount,
    decimal? MissionDaysCount);
