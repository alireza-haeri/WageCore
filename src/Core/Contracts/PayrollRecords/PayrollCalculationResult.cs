namespace Core.Contracts.PayrollRecords;

public record PayrollCalculationResult(
    decimal OvertimeAmount,
    decimal NightShiftExtraAmount,
    decimal FridayWorkAllowance,
    decimal CalculatedTaxAmount,
    decimal NetPayableAmount);
