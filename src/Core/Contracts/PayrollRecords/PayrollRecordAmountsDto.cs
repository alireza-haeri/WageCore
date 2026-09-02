namespace Core.Contracts.PayrollRecords;

public record PayrollRecordAmountsDto(
    decimal CalculatedTaxAmount,
    decimal GrossAmount,
    decimal InsuranceAmount,
    decimal TotalDeductionsAmount,
    decimal NetPayableAmount);
