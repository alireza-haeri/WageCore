using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.Department;

public class DepartmentQuery(IDbConnectionFactory dbConnectionFactory) : IDepartmentQuery
{
    public async Task<PagedResult<UserDepartmentResult>> GetUserDepartmentsAsync(Guid userId, PaginationDto pagination,
        string? searchName = null, Guid? workshopId = null, CancellationToken cancellationToken = default)
    {
        //todo get Employees count
        string sql = $"""
                             SELECT 
                                 d.Id AS DepartmentId, 
                                 d.Name, 
                                 d.WorkshopId,
                                 w.Name AS WorkshopName,
                                 0 AS EmployeesCount
                             FROM {Core.Domain.Department.TableName} d
                             INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = d.WorkshopId
                             WHERE w.UserId = @UserId
                             AND (@SearchName IS NULL OR d.Name LIKE '%' + @SearchName + '%')
                             AND (@WorkshopId IS NULL OR d.WorkshopId = @WorkshopId)
                             ORDER BY d.Name ASC, d.Id DESC
                             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                             SELECT COUNT(*)
                             FROM {Core.Domain.Department.TableName} d
                             INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = d.WorkshopId
                             WHERE w.UserId = @UserId
                             AND (@SearchName IS NULL OR d.Name LIKE '%' + @SearchName + '%')
                             AND (@WorkshopId IS NULL OR d.WorkshopId = @WorkshopId);
                             """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            SearchName = searchName,
            WorkshopId = workshopId,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);

        var departments = (await multi.ReadAsync<UserDepartmentResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<UserDepartmentResult>(departments, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<List<UserDepartmentNameResult>> GetUserDepartmentsNameAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const int maxDisplayNameLength = Core.Domain.Department.MaxDisplayNameLength;
        string sql = $"""
                      SELECT 
                          d.Id AS DepartmentId, 
                          CASE 
                             WHEN LEN(d.Name) > {maxDisplayNameLength} THEN LEFT(d.Name, {maxDisplayNameLength}) + N'...'
                             ELSE d.Name
                          END AS DisplayName
                      FROM {Core.Domain.Department.TableName} d
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = d.WorkshopId
                      WHERE w.UserId = @UserId
                      ORDER BY d.Name ASC, d.Id DESC;
                      """;

        var command = new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var departments = (await connection.QueryAsync<UserDepartmentNameResult>(command)).ToList();

        return departments;
    }

    public async Task<UserDepartmentByIdResult?> GetUserDepartmentByIdAsync(Guid userId, Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT 
                          d.Name AS Name,
                          d.WorkshopId AS WorkshopId
                      FROM {Core.Domain.Department.TableName} d
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = d.WorkshopId
                      WHERE w.UserId = @UserId AND d.Id = @DepartmentId;
                      """;

        var command = new CommandDefinition(sql, new { UserId = userId, DepartmentId = departmentId },
            cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var department = await connection.QueryFirstOrDefaultAsync<UserDepartmentByIdResult>(command);

        return department;
    }

    public async Task<bool> IsExistDepartmentName(Guid userId, string departmentName, Guid? excludeDepartmentId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                       SELECT CASE WHEN EXISTS (
                           SELECT 1
                           FROM {Core.Domain.Department.TableName} d
                           INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = d.WorkshopId
                           WHERE LOWER(TRIM(d.Name)) = LOWER(TRIM(@DepartmentName))
                           AND w.UserId = @UserId
                           AND (@ExcludeDepartmentId IS NULL OR d.Id <> @ExcludeDepartmentId)
                       ) THEN 1 ELSE 0 END
                       """;

        var command = new CommandDefinition(sql,
            new { DepartmentName = departmentName, ExcludeDepartmentId = excludeDepartmentId, UserId = userId },
            cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var exist = await connection.ExecuteScalarAsync<bool>(command);

        return exist;
    }
}
