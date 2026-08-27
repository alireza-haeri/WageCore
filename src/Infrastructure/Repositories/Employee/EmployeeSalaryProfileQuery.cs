namespace Infrastructure.Repositories.Employee;

public class EmployeeSalaryProfileQuery : IEmployeeSalaryProfileQuery
{
    public Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        Guid? excludeSalaryProfileId = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
