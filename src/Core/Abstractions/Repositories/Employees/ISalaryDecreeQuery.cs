namespace Core.Abstractions.Repositories.Employees;

public interface ISalaryDecreeQuery
{
    Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        Guid? excludeSalaryProfileId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SalaryDecree>> GetSalaryDecreesAffectingPeriodAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);

    Task<PagedResult<SalaryDecreeResult>> GetSalaryDecreesAsync(
        Guid userId,
        PaginationDto pagination,
        Guid? employeeId = null,
        string? search = null,
        SalaryDecreeStatus? status = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<SalaryDecreeByIdResult?> GetSalaryDecreeByIdAsync(
        Guid userId,
        Guid salaryDecreeId,
        CancellationToken cancellationToken = default);
}
