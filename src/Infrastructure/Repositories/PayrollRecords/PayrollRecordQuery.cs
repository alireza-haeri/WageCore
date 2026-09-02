using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.PayrollRecords;

public class PayrollRecordQuery(IDbConnectionFactory dbConnectionFactory) : IPayrollRecordQuery
{
    public async Task<bool> HasPayrollRecordEffectAsync(
        Guid userId,
        Guid employeeId,
        DateOnly effectiveFrom,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT CASE WHEN EXISTS (
                          SELECT 1
                          FROM {Core.Domain.PayrollRecord.TableName} pr
                          INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = pr.EmployeeId
                          INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                          WHERE w.UserId = @UserId
                          AND pr.EmployeeId = @EmployeeId
                          AND pr.PeriodEnd >= @EffectiveFrom
                      ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            EmployeeId = employeeId,
            EffectiveFrom = effectiveFrom
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<bool>(command);
    }

    public async Task<bool> HasOverlappingPeriodAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        Guid? excludePayrollRecordId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT CASE WHEN EXISTS (
                          SELECT 1
                          FROM {Core.Domain.PayrollRecord.TableName} pr
                          INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = pr.EmployeeId
                          INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                          WHERE w.UserId = @UserId
                          AND pr.EmployeeId = @EmployeeId
                          AND (@ExcludePayrollRecordId IS NULL OR pr.Id <> @ExcludePayrollRecordId)
                          AND pr.PeriodStart <= @PeriodEnd
                          AND pr.PeriodEnd >= @PeriodStart
                      ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            EmployeeId = employeeId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            ExcludePayrollRecordId = excludePayrollRecordId
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<bool>(command);
    }
}
