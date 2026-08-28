namespace Core.Contracts.PayrollRecords;

public record PayrollRecordAmountsDto(
    decimal OvertimeAmount,
    decimal NightShiftExtraAmount,
    decimal FridayWorkAllowance,
    decimal CalculatedTaxAmount,
    decimal NetPayableAmount);
