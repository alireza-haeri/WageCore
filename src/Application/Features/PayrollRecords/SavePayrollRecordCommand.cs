namespace Application.Features.PayrollRecords;

/// <summary>
/// Upsert keyed by (employee, Persian year, Persian month): creates the
/// payslip when none exists for the period, updates it otherwise.
/// </summary>
public record SavePayrollRecordCommand(
    Guid UserId,
    Guid EmployeeId,
    int PersianYear,
    int PersianMonth,
    UserWorkInputDto Work)
    : IRequest<Result<SavePayrollRecordCommandResponse>>;

public record SavePayrollRecordCommandResponse(Guid PayrollRecordId);
