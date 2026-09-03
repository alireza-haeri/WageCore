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

    Task<bool> HasOverlappingPeriodAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        Guid? excludePayrollRecordId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums <c>WorkedDaysCount</c> across the employee's already-persisted payroll
    /// records that fall inside the same Persian year as <paramref name="periodStart"/>
    /// and end before it (i.e. every closed period of that year except the current
    /// period being calculated). Pure aggregation; no business calculation is applied.
    /// </summary>
    Task<decimal> GetAnnualWorkedDaysCountAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the user's payroll records. <paramref name="search"/> matches the employee
    /// full name, personal code or national code. When <paramref name="periodStart"/> and
    /// <paramref name="periodEnd"/> are provided, only records whose period starts inside
    /// that range are returned (the Persian year/month filtering is resolved to a
    /// Gregorian range by the caller).
    /// </summary>
    Task<PagedResult<PayrollRecordResult>> GetPayrollRecordsAsync(
        Guid userId,
        PaginationDto pagination,
        string? search = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        DateOnly? periodStart = null,
        DateOnly? periodEnd = null,
        CancellationToken cancellationToken = default);
}
