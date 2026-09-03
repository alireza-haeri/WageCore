namespace Core.Contracts.PayrollRecords;

public record PayrollRecordResult(
    Guid PayrollRecordId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal WorkedDaysCount,
    decimal OvertimeHours,
    decimal GrossAmount,
    decimal TotalDeductionsAmount,
    decimal NetPayableAmount,
    PayrollRecordStatus Status);
