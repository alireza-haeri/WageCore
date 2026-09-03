namespace Application.Features.PayrollRecords;

public record CreatePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work)
    : IRequest<Result<CreatePayrollRecordCommandResponse>>;


public record CreatePayrollRecordCommandResponse(
    Guid PayrollRecordId,
    PayrollCalculatedAmountsDto CalculatedAmounts,
    PayrollRecordAmountsDto Amounts);