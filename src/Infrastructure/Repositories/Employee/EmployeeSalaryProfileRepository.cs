namespace Infrastructure.Repositories.Employee;

public class EmployeeSalaryProfileRepository : IEmployeeSalaryProfileRepository
{
    public Task<Guid?> CreateAsync(EmployeeSalaryProfile salaryProfile, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<EmployeeSalaryProfile?> GetByIdAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(EmployeeSalaryProfile salaryProfile, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
