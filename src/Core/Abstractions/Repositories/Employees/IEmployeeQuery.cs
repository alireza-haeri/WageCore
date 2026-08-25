namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeQuery
{
    Task<bool> IsExistEmployeePersonalCode(Guid workshopId, string personalCode, Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistEmployeeNationalCode(Guid userId, string nationalCode, Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default);
}
