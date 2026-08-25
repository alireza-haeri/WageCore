namespace Core.Abstractions.Repositories.Departments;

public interface IDepartmentQuery
{
    Task<PagedResult<UserDepartmentResult>> GetUserDepartmentsAsync(Guid userId, PaginationDto pagination,
        string? searchName = null, Guid? workshopId = null, CancellationToken cancellationToken = default);

    Task<List<UserDepartmentNameResult>> GetUserDepartmentsNameAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserDepartmentByIdResult?> GetUserDepartmentByIdAsync(Guid userId, Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistDepartmentName(Guid workshopId, string departmentName, Guid? excludeDepartmentId = null,
        CancellationToken cancellationToken = default);
}
