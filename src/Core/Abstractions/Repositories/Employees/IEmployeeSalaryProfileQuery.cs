namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeSalaryProfileQuery
{
    Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
