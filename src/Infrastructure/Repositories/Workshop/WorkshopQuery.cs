using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.Workshop;

public class WorkshopQuery(IDbConnectionFactory dbConnectionFactory) : IWorkshopQuery
{
    public async Task<PagedResult<UserWorkshopResult>> GetUserWorkshopsAsync(Guid userId, PaginationDto pagination,
        string? searchName = null,
        WorkshopRegion? region = null, CancellationToken cancellationToken = default)
    {
        //todo get Employees count and Departments count
        string sql = $"""
                             SELECT 
                                 Id AS WorkshopId, 
                                 Name, 
                                 Address, 
                                 Region, 
                                 RegistrationDate,
                                 0 AS EmployeesCount,
                                 0 AS DepartmentsCount
                             FROM {Core.Domain.Workshop.TableName}
                             WHERE UserId = @UserId
                             AND (@SearchName IS NULL OR Name LIKE '%' + @SearchName + '%')
                             AND (@Region IS NULL OR Region = @Region)
                             ORDER BY RegistrationDate DESC, Id DESC
                             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                             SELECT COUNT(*)
                             FROM {Core.Domain.Workshop.TableName}
                             WHERE UserId = @UserId
                             AND (@SearchName IS NULL OR Name LIKE '%' + @SearchName + '%')
                             AND (@Region IS NULL OR Region = @Region);
                             """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            SearchName = searchName,
            Region = region?.ToString(),
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
                          Region AS Region,
                          RegistrationDate AS RegistrationDate,
                          NationalId AS NationalId,
                          PostalCode AS PostalCode
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