using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.Workshop;

public class WorkshopQuery(IDbConnectionFactory dbConnectionFactory) : IWorkshopQuery
{
    public async Task<PagedResult<UserWorkshopResult>> GetUserWorkshopsAsync(Guid userId, PaginationDto pagination,
        string? searchName = null, CancellationToken cancellationToken = default)
    {
        string sql = $"""
                             SELECT 
                                 w.Id AS WorkshopId, 
                                 w.Name, 
                                 w.Address, 
                                 w.NationalId,
                                 w.RegistrationDate,
                                 (
                                     SELECT COUNT(*)
                                     FROM {Core.Domain.Employee.TableName} e
                                     WHERE e.WorkshopId = w.Id
                                 ) AS EmployeesCount,
                                 (
                                     SELECT COUNT(*)
                                     FROM {Core.Domain.Department.TableName} d
                                     WHERE d.WorkshopId = w.Id
                                 ) AS DepartmentsCount,
                                 w.SocialSecurityNumber,
                                 w.EconomicCode
                             FROM {Core.Domain.Workshop.TableName} w
                             WHERE w.UserId = @UserId
                             AND (@SearchName IS NULL OR w.Name LIKE '%' + @SearchName + '%')
                             ORDER BY w.RegistrationDate DESC, w.Id DESC
                             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                             SELECT COUNT(*)
                             FROM {Core.Domain.Workshop.TableName} w
                             WHERE w.UserId = @UserId
                             AND (@SearchName IS NULL OR w.Name LIKE '%' + @SearchName + '%');
                             """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            SearchName = searchName,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);
        
        var workshops = (await multi.ReadAsync<UserWorkshopResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<UserWorkshopResult>(workshops, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<List<UserWorkshopNameResult>> GetUserWorkshopsNameAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        const int maxDisplayNameLength = Core.Domain.Workshop.MaxDisplayNameLength;
        string sql = $"""
                      SELECT 
                          Id AS WorkshopId, 
                          CASE 
                             WHEN LEN(Name) > {maxDisplayNameLength} THEN LEFT(Name, {maxDisplayNameLength}) + N'...'
                             ELSE Name
                          END AS DisplayName
                      FROM {Core.Domain.Workshop.TableName}
                      WHERE UserId = @UserId
                      ORDER BY RegistrationDate DESC, Id DESC;
                      """;

        var command = new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var workshops = (await connection.QueryAsync<UserWorkshopNameResult>(command)).ToList();

        return workshops;
    }

    public async Task<UserWorkshopByIdResult?> GetUserWorkshopByIdAsync(Guid userId, Guid workshopId, CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT 
                          Name AS Name,
                          Address AS Address,
                          RegistrationDate AS RegistrationDate,
                          NationalId AS NationalId,
                          SocialSecurityNumber AS SocialSecurityNumber,
                          PostalCode AS PostalCode,
                          EconomicCode AS EconomicCode
                      FROM {Core.Domain.Workshop.TableName}
                      WHERE UserId = @UserId AND Id = @WorkshopId;
                      """;

        var command = new CommandDefinition(sql, new { UserId = userId, WorkshopId = workshopId }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var workshop = await connection.QueryFirstOrDefaultAsync<UserWorkshopByIdResult>(command);

        return workshop;
    }

    public async Task<bool> IsExistWorkshopName(Guid userId, string workshopName, Guid? excludeWorkshopId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                       SELECT CASE WHEN EXISTS (
                           SELECT 1
                           FROM {Core.Domain.Workshop.TableName}
                           WHERE LOWER(TRIM(Name)) = LOWER(TRIM(@WorkshopName))
                           AND UserId = @UserId
                           AND (@ExcludeWorkshopId IS NULL OR Id <> @ExcludeWorkshopId)
                       ) THEN 1 ELSE 0 END
                       """;

        var command = new CommandDefinition(sql,
            new { WorkshopName = workshopName, ExcludeWorkshopId = excludeWorkshopId, UserId = userId }
            , cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var exist = await connection.ExecuteScalarAsync<bool>(command);

        return exist;
    }

    public async Task<bool> IsExistWorkshopNationalId(Guid userId,string nationalId, Guid? excludeWorkshopId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                       SELECT CASE WHEN EXISTS (
                           SELECT 1
                           FROM {Core.Domain.Workshop.TableName}
                           WHERE LOWER(TRIM(NationalId)) = LOWER(TRIM(@NationalId))
                           AND UserId = @UserId
                           AND (@ExcludeWorkshopId IS NULL OR Id <> @ExcludeWorkshopId)
                       ) THEN 1 ELSE 0 END
                       """;

        var command = new CommandDefinition(sql,
            new { NationalId = nationalId, ExcludeWorkshopId = excludeWorkshopId, UserId = userId }
            , cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var exist = await connection.ExecuteScalarAsync<bool>(command);

        return exist;
    }
}