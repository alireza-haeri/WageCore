namespace Application.Features.PayrollRecords;

public record CreatePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    PayrollRecordDto PayrollRecord)
    : IRequest<Result<CreatePayrollRecordCommandResponse>>;

public record CreatePayrollRecordCommandResponse(Guid PayrollRecordId);
