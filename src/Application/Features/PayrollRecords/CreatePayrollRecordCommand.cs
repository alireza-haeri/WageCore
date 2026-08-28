namespace Application.Features.PayrollRecords;

public record CreatePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    int PersianYear,
    int PersianMonth,
    PayrollRecordDto PayrollRecord)
    : IRequest<Result<CreatePayrollRecordCommandResponse>>;

public record CreatePayrollRecordCommandResponse(Guid PayrollRecordId);
