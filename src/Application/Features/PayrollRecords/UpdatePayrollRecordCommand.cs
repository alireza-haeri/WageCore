namespace Application.Features.PayrollRecords;

public record UpdatePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid PayrollRecordId,
    int PersianYear,
    int PersianMonth,
    PayrollWorkInputDto Work)
    : IRequest<Result<bool>>;
