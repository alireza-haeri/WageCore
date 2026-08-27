namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeSalaryProfileRepository
{
    Task<Guid?> CreateAsync(EmployeeSalaryProfile salaryProfile, CancellationToken cancellationToken = default);

    Task<EmployeeSalaryProfile?> GetByIdAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(EmployeeSalaryProfile salaryProfile, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken = default);
}
