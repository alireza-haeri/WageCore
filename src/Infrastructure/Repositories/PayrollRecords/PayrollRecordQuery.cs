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

    public async Task<PagedResult<PayrollRecordResult>> GetPayrollRecordsAsync(
        Guid userId,
        PaginationDto pagination,
        string? search = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        DateOnly? periodStart = null,
        DateOnly? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          pr.Id AS PayrollRecordId,
                          pr.EmployeeId,
                          e.FullName AS EmployeeName,
                          e.PersonalCode,
                          w.Name AS WorkshopName,
                          d.Name AS DepartmentName,
                          pr.PeriodStart,
                          pr.PeriodEnd,
                          pr.WorkedDaysCount,
                          pr.OvertimeHours,
                          pr.GrossAmount,
                          pr.TotalDeductionsAmount,
                          pr.NetPayableAmount,
                          pr.Status
                      FROM {Core.Domain.PayrollRecord.TableName} pr
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = pr.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      INNER JOIN {Core.Domain.Department.TableName} d ON d.Id = e.DepartmentId AND d.WorkshopId = e.WorkshopId
                      WHERE w.UserId = @UserId
                      AND (@Search IS NULL OR
                          e.FullName LIKE '%' + @Search + '%' OR
                          e.PersonalCode LIKE '%' + @Search + '%' OR
                          e.NationalCode LIKE '%' + @Search + '%')
                      AND (@WorkshopId IS NULL OR e.WorkshopId = @WorkshopId)
                      AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
                      AND (@PeriodStart IS NULL OR (pr.PeriodStart >= @PeriodStart AND pr.PeriodStart <= @PeriodEnd))
                      ORDER BY pr.PeriodStart DESC, pr.Id DESC
                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                      SELECT COUNT(*)
                      FROM {Core.Domain.PayrollRecord.TableName} pr
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = pr.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      INNER JOIN {Core.Domain.Department.TableName} d ON d.Id = e.DepartmentId AND d.WorkshopId = e.WorkshopId
                      WHERE w.UserId = @UserId
                      AND (@Search IS NULL OR
                          e.FullName LIKE '%' + @Search + '%' OR
                          e.PersonalCode LIKE '%' + @Search + '%' OR
                          e.NationalCode LIKE '%' + @Search + '%')
                      AND (@WorkshopId IS NULL OR e.WorkshopId = @WorkshopId)
                      AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
                      AND (@PeriodStart IS NULL OR (pr.PeriodStart >= @PeriodStart AND pr.PeriodStart <= @PeriodEnd));
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            Search = search,
            WorkshopId = workshopId,
            DepartmentId = departmentId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);

        var payrollRecords = (await multi.ReadAsync<PayrollRecordResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<PayrollRecordResult>(
            payrollRecords, totalCount, pagination.PageNumber, pagination.PageSize);
    }
}
