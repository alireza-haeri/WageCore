namespace Core.Contracts.PayrollRecords;

public record PayrollCalculationResult(
    PayrollCalculatedAmountsDto CalculatedAmounts,
    PayrollRecordAmountsDto Amounts,
    bool IsEsfandPeriod);
