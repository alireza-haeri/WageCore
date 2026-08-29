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

    /// <summary>
    /// Checks whether the employee already has a payroll record whose period overlaps
    /// <paramref name="periodStart"/>..<paramref name="periodEnd"/>. Pass
    /// <paramref name="excludePayrollRecordId"/> when updating a record, so it does not
    /// overlap with itself.
    /// </summary>
    Task<bool> HasOverlappingPeriodAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        Guid? excludePayrollRecordId = null,
        CancellationToken cancellationToken = default);
}
