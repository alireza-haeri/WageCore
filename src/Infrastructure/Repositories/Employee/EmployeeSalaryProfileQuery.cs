using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.Employee;

public class EmployeeSalaryProfileQuery(IDbConnectionFactory dbConnectionFactory) : IEmployeeSalaryProfileQuery
{
    public async Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        Guid? excludeSalaryProfileId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT TOP 1 sp.EffectiveFrom
                      FROM {Core.Domain.EmployeeSalaryProfile.TableName} sp
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = sp.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      WHERE w.UserId = @UserId
                      AND sp.EmployeeId = @EmployeeId
                      AND (@ExcludeSalaryProfileId IS NULL OR sp.Id <> @ExcludeSalaryProfileId)
                      ORDER BY sp.EffectiveFrom DESC, sp.Id DESC;
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            EmployeeId = employeeId,
            ExcludeSalaryProfileId = excludeSalaryProfileId
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var latestEffectiveFrom = await connection.QueryFirstOrDefaultAsync<DateTime?>(command);

        return latestEffectiveFrom is null ? null : DateOnly.FromDateTime(latestEffectiveFrom.Value);
    }

    public Task<IReadOnlyList<EmployeeSalaryProfile>> GetEmployeeSalaryProfilesAffectingPeriodAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<EmployeeSalaryProfileResult>> GetEmployeeSalaryProfilesAsync(
        Guid userId,
        PaginationDto pagination,
        Guid? employeeId = null,
        string? search = null,
        EmployeeSalaryProfileStatus? status = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          sp.Id AS EmployeeSalaryProfileId,
                          sp.EmployeeId,
                          e.FullName AS EmployeeName,
                          e.PersonalCode,
                          w.Name AS WorkshopName,
                          d.Name AS DepartmentName,
                          sp.EffectiveFrom,
                          sp.BaseMonthlySalary,
                          CASE WHEN EXISTS (
                              SELECT 1
                              FROM {Core.Domain.EmployeeSalaryProfile.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          ) THEN {(int)EmployeeSalaryProfileStatus.Expired}
                          ELSE {(int)EmployeeSalaryProfileStatus.Active}
                          END AS Status
                      FROM {Core.Domain.EmployeeSalaryProfile.TableName} sp
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = sp.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      INNER JOIN {Core.Domain.Department.TableName} d ON d.Id = e.DepartmentId AND d.WorkshopId = e.WorkshopId
                      WHERE w.UserId = @UserId
                      AND (@EmployeeId IS NULL OR sp.EmployeeId = @EmployeeId)
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
                          (@Status = {(int)EmployeeSalaryProfileStatus.Active} AND NOT EXISTS (
                              SELECT 1
                              FROM {Core.Domain.EmployeeSalaryProfile.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          )) OR
                          (@Status = {(int)EmployeeSalaryProfileStatus.Expired} AND EXISTS (
                              SELECT 1
                              FROM {Core.Domain.EmployeeSalaryProfile.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          ))
                      )
                      ORDER BY sp.EffectiveFrom DESC, sp.Id DESC
                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                      SELECT COUNT(*)
                      FROM {Core.Domain.EmployeeSalaryProfile.TableName} sp
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = sp.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      INNER JOIN {Core.Domain.Department.TableName} d ON d.Id = e.DepartmentId AND d.WorkshopId = e.WorkshopId
                      WHERE w.UserId = @UserId
                      AND (@EmployeeId IS NULL OR sp.EmployeeId = @EmployeeId)
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
                          (@Status = {(int)EmployeeSalaryProfileStatus.Active} AND NOT EXISTS (
                              SELECT 1
                              FROM {Core.Domain.EmployeeSalaryProfile.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          )) OR
                          (@Status = {(int)EmployeeSalaryProfileStatus.Expired} AND EXISTS (
                              SELECT 1
                              FROM {Core.Domain.EmployeeSalaryProfile.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          ))
                      );
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            EmployeeId = employeeId,
            Search = search,
            Status = (int?)status,
            WorkshopId = workshopId,
            DepartmentId = departmentId,
            Offset = pagination.Offset,
            PageSize = pagination.PageSize
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);

        var salaryProfiles = (await multi.ReadAsync<EmployeeSalaryProfileResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<EmployeeSalaryProfileResult>(
            salaryProfiles, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<EmployeeSalaryProfileByIdResult?> GetEmployeeSalaryProfileByIdAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          sp.EmployeeId,
                          sp.EffectiveFrom,
                          sp.BaseMonthlySalary,
                          sp.AttractionAllowance,
                          sp.SupervisionAllowance,
                          sp.SeniorityBaseApplicationMode,
                          sp.SeniorityBaseCalculationMethod,
                          sp.YearEndSeniorityMode,
                          sp.ShiftType,
                          sp.HousingAllowance,
                          sp.FoodAllowance,
                          sp.ChildAllowancePerChild,
                          sp.TransportationAllowanceNet,
                          sp.KaranehAmountNet
                      FROM {Core.Domain.EmployeeSalaryProfile.TableName} sp
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = sp.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      WHERE w.UserId = @UserId AND sp.Id = @EmployeeSalaryProfileId;
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            EmployeeSalaryProfileId = employeeSalaryProfileId
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var salaryProfile = await connection.QueryFirstOrDefaultAsync<EmployeeSalaryProfileByIdDbResult>(command);

        if (salaryProfile is null)
            return null;

        return new EmployeeSalaryProfileByIdResult(
            salaryProfile.EmployeeId,
            salaryProfile.EffectiveFrom,
            salaryProfile.BaseMonthlySalary,
            salaryProfile.AttractionAllowance,
            salaryProfile.SupervisionAllowance,
            Enum.Parse<SeniorityBaseApplicationMode>(salaryProfile.SeniorityBaseApplicationMode),
            salaryProfile.SeniorityBaseCalculationMethod is null
                ? (SeniorityBaseCalculationMethod?)null
                : Enum.Parse<SeniorityBaseCalculationMethod>(salaryProfile.SeniorityBaseCalculationMethod),
            Enum.Parse<YearEndSeniorityMode>(salaryProfile.YearEndSeniorityMode),
            Enum.Parse<ShiftType>(salaryProfile.ShiftType),
            salaryProfile.HousingAllowance,
            salaryProfile.FoodAllowance,
            salaryProfile.ChildAllowancePerChild,
            salaryProfile.TransportationAllowanceNet,
            salaryProfile.KaranehAmountNet);
    }

    private sealed class EmployeeSalaryProfileByIdDbResult
    {
        public Guid EmployeeId { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public decimal BaseMonthlySalary { get; set; }
        public decimal? AttractionAllowance { get; set; }
        public decimal? SupervisionAllowance { get; set; }
        public string SeniorityBaseApplicationMode { get; set; } = null!;
        public string? SeniorityBaseCalculationMethod { get; set; }
        public string YearEndSeniorityMode { get; set; } = null!;
        public string ShiftType { get; set; } = null!;
        public decimal? HousingAllowance { get; set; }
        public decimal? FoodAllowance { get; set; }
        public decimal? ChildAllowancePerChild { get; set; }
        public decimal? TransportationAllowanceNet { get; set; }
        public decimal? KaranehAmountNet { get; set; }
    }
}
