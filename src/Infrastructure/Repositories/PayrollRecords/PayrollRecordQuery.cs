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

    public async Task<decimal> GetAnnualWorkedDaysCountAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        CancellationToken cancellationToken = default)
    {
        var persianCalendar = new System.Globalization.PersianCalendar();
        var startDateTime = periodStart.ToDateTime(TimeOnly.MinValue);
        var persianYear = persianCalendar.GetYear(startDateTime);
        var yearStart = DateOnly.FromDateTime(
            persianCalendar.ToDateTime(persianYear, 1, 1, 0, 0, 0, 0));

        // Aggregates the employee's closed periods of the same Persian year only:
        // any period that has not ended before the current one started (including
        // the payroll record being (re)calculated right now) is left out.
        string sql = $"""
                      SELECT ISNULL(SUM(pr.WorkedDaysCount), 0)
                      FROM {Core.Domain.PayrollRecord.TableName} pr
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = pr.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      WHERE w.UserId = @UserId
                      AND pr.EmployeeId = @EmployeeId
                      AND pr.PeriodStart >= @YearStart
                      AND pr.PeriodEnd < @PeriodStart;
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            EmployeeId = employeeId,
            PeriodStart = periodStart,
            YearStart = yearStart
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<decimal>(command);
    }
}
