namespace Application.Services;

public record PayrollCalculationResult(
    decimal? MaxMonthlyOvertimeHours,
    decimal? MaxFridayHours,
    decimal OvertimeAmount,
    decimal NightShiftExtraAmount,
    decimal FridayWorkAllowance,
    decimal CalculatedTaxAmount,
    decimal NetPayableAmount);
