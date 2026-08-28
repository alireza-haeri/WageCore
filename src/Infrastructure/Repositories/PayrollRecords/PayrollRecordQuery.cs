namespace Infrastructure.Repositories.PayrollRecords;

// TODO: Implement the real payroll-record effect check once the PayrollRecord domain exists.
public class PayrollRecordQuery : IPayrollRecordQuery
{
    public Task<bool> HasPayrollRecordEffectAsync(
        Guid userId,
        Guid employeeId,
        DateOnly effectiveFrom,
        CancellationToken cancellationToken = default)
    {
        // TODO: Replace this placeholder with the actual query against the PayrollRecord table.
        // Returning false for now so the employee salary profile flows are not blocked.
        return Task.FromResult(false);
    }
}
