namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeSalaryProfileRepository
{
    Task<Guid?> CreateAsync(EmployeeSalaryProfile salaryProfile, CancellationToken cancellationToken = default);
}
