namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeSalaryProfileQuery
{
    Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        Guid? excludeSalaryProfileId = null,
        CancellationToken cancellationToken = default);
}
