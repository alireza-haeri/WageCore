namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeRepository
{
    Task<Guid?> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
}
