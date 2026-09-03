namespace Web.Api.Controllers.PayrollRecords.Contracts;

public record CreatePayrollRecordRequest(
    Guid EmployeeId,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work);

public record UpdatePayrollRecordRequest(
    Guid EmployeeId,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work);
