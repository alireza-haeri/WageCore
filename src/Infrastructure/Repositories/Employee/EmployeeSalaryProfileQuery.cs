namespace Infrastructure.Repositories.Employee;

public class EmployeeSalaryProfileQuery : IEmployeeSalaryProfileQuery
{
    public Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
