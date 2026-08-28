namespace Core.Abstractions.Repositories.Employees;

public interface IEmployeeSalaryProfileQuery
{
    Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        Guid? excludeSalaryProfileId = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<EmployeeSalaryProfileResult>> GetEmployeeSalaryProfilesAsync(
        Guid userId,
        PaginationDto pagination,
        Guid? employeeId = null,
        string? search = null,
        EmployeeSalaryProfileStatus? status = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<EmployeeSalaryProfileByIdResult?> GetEmployeeSalaryProfileByIdAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken = default);
}
