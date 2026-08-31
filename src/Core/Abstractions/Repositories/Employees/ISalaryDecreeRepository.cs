namespace Core.Abstractions.Repositories.Employees;

public interface ISalaryDecreeRepository
{
    Task<Guid?> CreateAsync(SalaryDecree salaryProfile, CancellationToken cancellationToken = default);

    Task<SalaryDecree?> GetByIdAsync(
        Guid userId,
        Guid salaryDecreeId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(SalaryDecree salaryProfile, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid salaryDecreeId,
        CancellationToken cancellationToken = default);
}
