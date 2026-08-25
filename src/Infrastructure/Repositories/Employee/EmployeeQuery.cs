using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.Employee;

public class EmployeeQuery(IDbConnectionFactory dbConnectionFactory) : IEmployeeQuery
{
    public async Task<PagedResult<UserEmployeeResult>> GetUserEmployeesAsync(Guid userId, PaginationDto pagination,
        string? search = null, Guid? workshopId = null, Guid? departmentId = null, EmployeeStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                             SELECT 
                                 e.Id AS EmployeeId,
                                 e.PersonalCode,
                                 e.FullName,
                                 w.Name AS WorkshopName,
                                 d.Name AS DepartmentName,
                                 e.NationalCode,
                                 e.HireDate,
                                 e.JobTitle,
                                 CASE 
                                     WHEN e.TerminationDate IS NULL THEN {(int)EmployeeStatus.Employed}
                                     ELSE {(int)EmployeeStatus.Unemployed}
                                 END AS Status
                             FROM {Core.Domain.Employee.TableName} e
                             INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                             INNER JOIN {Core.Domain.Department.TableName} d ON d.Id = e.DepartmentId AND d.WorkshopId = e.WorkshopId
                             WHERE w.UserId = @UserId
                             AND (
                                 @Search IS NULL OR
                                 e.FullName LIKE '%' + @Search + '%' OR
                                 e.PersonalCode LIKE '%' + @Search + '%' OR
                                 e.NationalCode LIKE '%' + @Search + '%'
                             )
                             AND (@WorkshopId IS NULL OR e.WorkshopId = @WorkshopId)
                             AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
                             AND (
                                 @Status IS NULL OR
                                 (@Status = {(int)EmployeeStatus.Employed} AND e.TerminationDate IS NULL) OR
                                 (@Status = {(int)EmployeeStatus.Unemployed} AND e.TerminationDate IS NOT NULL)
                             )
                             ORDER BY e.FullName ASC, e.Id DESC
                             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                             SELECT COUNT(*)
                             FROM {Core.Domain.Employee.TableName} e
                             INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                             INNER JOIN {Core.Domain.Department.TableName} d ON d.Id = e.DepartmentId AND d.WorkshopId = e.WorkshopId
                             WHERE w.UserId = @UserId
                             AND (
                                 @Search IS NULL OR
                                 e.FullName LIKE '%' + @Search + '%' OR
                                 e.PersonalCode LIKE '%' + @Search + '%' OR
                                 e.NationalCode LIKE '%' + @Search + '%'
                             )
                             AND (@WorkshopId IS NULL OR e.WorkshopId = @WorkshopId)
                             AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
                             AND (
                                 @Status IS NULL OR
                                 (@Status = {(int)EmployeeStatus.Employed} AND e.TerminationDate IS NULL) OR
                                 (@Status = {(int)EmployeeStatus.Unemployed} AND e.TerminationDate IS NOT NULL)
                             );
                             """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            Search = search,
            WorkshopId = workshopId,
            DepartmentId = departmentId,
            Status = (int?)status,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);

        var employees = (await multi.ReadAsync<UserEmployeeResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<UserEmployeeResult>(employees, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<bool> IsExistEmployeePersonalCode(Guid userId, string personalCode, Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                       SELECT CASE WHEN EXISTS (
                           SELECT 1
                           FROM {Core.Domain.Employee.TableName} e
                           INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                           WHERE LOWER(TRIM(e.PersonalCode)) = LOWER(TRIM(@PersonalCode))
                           AND w.UserId = @UserId
                           AND (@ExcludeEmployeeId IS NULL OR e.Id <> @ExcludeEmployeeId)
                       ) THEN 1 ELSE 0 END
                       """;

        var command = new CommandDefinition(sql,
            new { UserId = userId, PersonalCode = personalCode, ExcludeEmployeeId = excludeEmployeeId },
            cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var exist = await connection.ExecuteScalarAsync<bool>(command);

        return exist;
    }

    public async Task<bool> IsExistEmployeeNationalCode(Guid userId, string nationalCode, Guid? excludeEmployeeId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                       SELECT CASE WHEN EXISTS (
                           SELECT 1
                           FROM {Core.Domain.Employee.TableName} e
                           INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                           WHERE LOWER(TRIM(e.NationalCode)) = LOWER(TRIM(@NationalCode))
                           AND w.UserId = @UserId
                           AND (@ExcludeEmployeeId IS NULL OR e.Id <> @ExcludeEmployeeId)
                       ) THEN 1 ELSE 0 END
                       """;

        var command = new CommandDefinition(sql,
            new { UserId = userId, NationalCode = nationalCode, ExcludeEmployeeId = excludeEmployeeId },
            cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var exist = await connection.ExecuteScalarAsync<bool>(command);

        return exist;
    }
}
