namespace Application.Features.PayrollRecords;

public record GetPayrollRecordForEditQuery(Guid UserId, Guid PayrollRecordId)
    : IRequest<Result<GetPayrollRecordForEditQueryResponse>>;

public record GetPayrollRecordForEditQueryResponse(
    Guid PayrollRecordId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work,
    PayrollRecordStatus Status);
