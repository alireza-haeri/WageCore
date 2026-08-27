namespace Infrastructure.Repositories.Employee;

public class EmployeeSalaryProfileRepository : IEmployeeSalaryProfileRepository
{
    public Task<Guid?> CreateAsync(EmployeeSalaryProfile salaryProfile, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
