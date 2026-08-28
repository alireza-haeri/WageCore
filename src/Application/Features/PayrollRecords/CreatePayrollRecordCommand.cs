namespace Application.Features.PayrollRecords;

public record CreatePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    int PersianYear,
    int PersianMonth,
    PayrollWorkInputDto Work)
    : IRequest<Result<CreatePayrollRecordCommandResponse>>;

public record CreatePayrollRecordCommandResponse(Guid PayrollRecordId);
