namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeRepository
{
    Task<Guid?> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(Guid userId, Guid employeeId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, Guid employeeId, CancellationToken cancellationToken = default);
}
