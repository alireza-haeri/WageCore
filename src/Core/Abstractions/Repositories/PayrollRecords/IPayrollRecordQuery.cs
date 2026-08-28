namespace Core.Abstractions.Repositories.PayrollRecords;

/// <summary>
/// Read-only access to the PayrollRecord domain.
/// </summary>
public interface IPayrollRecordQuery
{
    /// <summary>
    /// Checks whether the employee's salary, effective from <paramref name="effectiveFrom"/>,
    /// affects any existing payroll record. When it does, the salary profile cannot be
    /// updated or deleted.
    /// </summary>
    Task<bool> HasPayrollRecordEffectAsync(
        Guid userId,
        Guid employeeId,
        DateOnly effectiveFrom,
        CancellationToken cancellationToken = default);
}
