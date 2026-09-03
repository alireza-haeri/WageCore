namespace Web.Api.Controllers.PayrollRecords.Contracts;

public record GetPayrollRecordsRequest(
    PaginationDto Pagination,
    string? Search = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null,
    int? PersianYear = null,
    int? PersianMonth = null);

public record GetPayrollRecordsResponse(
    Guid PayrollRecordId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    string DisplayPeriod,
    decimal WorkedDaysCount,
    decimal OvertimeHours,
    decimal GrossAmount,
    decimal TotalDeductionsAmount,
    decimal NetPayableAmount,
    PayrollRecordStatus Status);

public record GetPayrollRecordForEditResponse(
    Guid PayrollRecordId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work,
    decimal OvertimeAmount,
    decimal NightShiftExtraAmount,
    decimal FridayWorkAllowance,
    PayrollRecordAmountsDto Amounts,
    PayrollRecordStatus Status);
