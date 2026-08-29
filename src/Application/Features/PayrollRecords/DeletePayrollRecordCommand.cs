namespace Application.Features.PayrollRecords;

public record DeletePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid PayrollRecordId)
    : IRequest<Result<bool>>;
