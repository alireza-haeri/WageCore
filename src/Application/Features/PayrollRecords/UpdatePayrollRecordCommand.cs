namespace Application.Features.PayrollRecords;

public record UpdatePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid PayrollRecordId,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work)
    : IRequest<Result<bool>>;