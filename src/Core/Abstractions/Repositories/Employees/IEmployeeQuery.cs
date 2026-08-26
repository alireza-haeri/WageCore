namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeQuery
{
    Task<PagedResult<UserEmployeeResult>> GetUserEmployeesAsync(
        Guid userId,
        PaginationDto pagination,
        string? search = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        EmployeeStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<UserEmployeeByIdResult?> GetUserEmployeeByIdAsync(Guid userId, Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistEmployeePersonalCode(Guid userId, string personalCode, Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistEmployeeNationalCode(Guid userId, string nationalCode, Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default);
}
