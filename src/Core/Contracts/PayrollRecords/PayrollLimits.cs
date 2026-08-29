namespace Core.Contracts.PayrollRecords;

public record PayrollLimits(
    decimal MaxMonthlyOvertimeHours,
    decimal MaxFridayHours,
    decimal MaxNightShiftHours);
