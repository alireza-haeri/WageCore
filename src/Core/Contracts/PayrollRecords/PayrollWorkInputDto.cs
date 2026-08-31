namespace Core.Contracts.PayrollRecords;

public record PayrollWorkInputDto(
    decimal? WorkedDaysCount,
    decimal? OvertimeHours,
    decimal? NightShiftHours,
    decimal? FridayWorkHours,
    decimal? LeaveDaysCount,
    decimal? AbsenceDaysCount,
    decimal? MissionDaysCount);
