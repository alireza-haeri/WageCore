namespace Web.Api.Controllers.PayrollRecords.Contracts;

public record SavePayrollRecordRequest(
    Guid EmployeeId,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work);
