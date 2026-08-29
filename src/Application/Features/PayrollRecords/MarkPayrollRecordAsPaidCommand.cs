namespace Application.Features.PayrollRecords;

public record MarkPayrollRecordAsPaidCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid PayrollRecordId)
    : IRequest<Result<bool>>;
