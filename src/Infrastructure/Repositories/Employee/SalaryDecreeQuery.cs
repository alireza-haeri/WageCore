using Infrastructure.Persistence.Dapper;

namespace Infrastructure.Repositories.Employee;

public class SalaryDecreeQuery(IDbConnectionFactory dbConnectionFactory) : ISalaryDecreeQuery
{
    public async Task<DateOnly?> GetLatestEffectiveFromAsync(
        Guid userId,
        Guid employeeId,
        Guid? excludeSalaryProfileId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT TOP 1 sp.EffectiveFrom
                      FROM {Core.Domain.SalaryDecree.TableName} sp
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

    public Task<IReadOnlyList<SalaryDecree>> GetSalaryDecreesAffectingPeriodAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResult<SalaryDecreeResult>> GetSalaryDecreesAsync(
        Guid userId,
        PaginationDto pagination,
        Guid? employeeId = null,
        string? search = null,
        SalaryDecreeStatus? status = null,
        Guid? workshopId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          sp.Id AS SalaryDecreeId,
                          sp.EmployeeId,
                          e.FullName AS EmployeeName,
                          e.PersonalCode,
                          w.Name AS WorkshopName,
                          d.Name AS DepartmentName,
                          sp.EffectiveFrom,
                          sp.BaseDailySalary,
                          CASE WHEN EXISTS (
                              SELECT 1
                              FROM {Core.Domain.SalaryDecree.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          ) THEN {(int)SalaryDecreeStatus.Expired}
                          ELSE {(int)SalaryDecreeStatus.Active}
                          END AS Status
                      FROM {Core.Domain.SalaryDecree.TableName} sp
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
                          (@Status = {(int)SalaryDecreeStatus.Active} AND NOT EXISTS (
                              SELECT 1
                              FROM {Core.Domain.SalaryDecree.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          )) OR
                          (@Status = {(int)SalaryDecreeStatus.Expired} AND EXISTS (
                              SELECT 1
                              FROM {Core.Domain.SalaryDecree.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          ))
                      )
                      ORDER BY sp.EffectiveFrom DESC, sp.Id DESC
                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                      SELECT COUNT(*)
                      FROM {Core.Domain.SalaryDecree.TableName} sp
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
                          (@Status = {(int)SalaryDecreeStatus.Active} AND NOT EXISTS (
                              SELECT 1
                              FROM {Core.Domain.SalaryDecree.TableName} later
                              WHERE later.EmployeeId = sp.EmployeeId
                              AND later.EffectiveFrom > sp.EffectiveFrom
                          )) OR
                          (@Status = {(int)SalaryDecreeStatus.Expired} AND EXISTS (
                              SELECT 1
                              FROM {Core.Domain.SalaryDecree.TableName} later
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

        var salaryProfiles = (await multi.ReadAsync<SalaryDecreeResult>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<SalaryDecreeResult>(
            salaryProfiles, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<SalaryDecreeByIdResult?> GetSalaryDecreeByIdAsync(
        Guid userId,
        Guid salaryDecreeId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          sp.EmployeeId,
                          sp.EffectiveFrom,
                          sp.BaseDailySalary,
                          sp.AttractionAllowance,
                          sp.SupervisionAllowance,
                          sp.ShiftType,
                          sp.ContractType,
                          sp.HousingAllowance,
                          sp.FoodAllowance,
                          sp.TransportationAllowanceNet,
                          sp.KaranehAmountNet,
                          sp.MaritalStatus,
                          sp.ChildrenCount,
                          sp.IsTaxSubject,
                          sp.InsuranceNumber AS InsuranceNumber,
                          sp.SocialSecurityContractRow AS SocialSecurityContractRow,
                          sp.PositionInInsuranceList AS PositionInInsuranceList,
                          sp.IsSubjectTo7PercentInsurance AS IsSubjectTo7PercentInsurance,
                          sp.IsSubjectTo20PercentInsurance AS IsSubjectTo20PercentInsurance,
                          sp.IsSubjectTo3PercentInsurance AS IsSubjectTo3PercentInsurance,
                          sp.IsSubjectTo4PercentInsurance AS IsSubjectTo4PercentInsurance,
                          sp.InsuranceCalculationProfile AS InsuranceCalculationProfile
                      FROM {Core.Domain.SalaryDecree.TableName} sp
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = sp.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      WHERE w.UserId = @UserId AND sp.Id = @SalaryDecreeId;
                      """;

        var command = new CommandDefinition(sql, new
        {
            UserId = userId,
            SalaryDecreeId = salaryDecreeId
        }, cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        var salaryProfile = await connection.QueryFirstOrDefaultAsync<SalaryDecreeByIdDbResult>(command);

        if (salaryProfile is null)
            return null;

        return new SalaryDecreeByIdResult(
            salaryProfile.EmployeeId,
            salaryProfile.EffectiveFrom,
            salaryProfile.BaseDailySalary,
            salaryProfile.AttractionAllowance,
            salaryProfile.SupervisionAllowance,
            Enum.Parse<ShiftType>(salaryProfile.ShiftType),
            Enum.Parse<ContractType>(salaryProfile.ContractType),
            salaryProfile.HousingAllowance,
            salaryProfile.FoodAllowance,
            salaryProfile.TransportationAllowanceNet,
            salaryProfile.KaranehAmountNet,
            Enum.Parse<EmployeeMaritalStatus>(salaryProfile.MaritalStatus),
            salaryProfile.ChildrenCount,
            salaryProfile.IsTaxSubject,
            salaryProfile.InsuranceNumber,
            salaryProfile.SocialSecurityContractRow,
            salaryProfile.PositionInInsuranceList,
            salaryProfile.IsSubjectTo7PercentInsurance,
            salaryProfile.IsSubjectTo20PercentInsurance,
            salaryProfile.IsSubjectTo3PercentInsurance,
            salaryProfile.IsSubjectTo4PercentInsurance,
            Enum.Parse<InsuranceCalculationProfile>(salaryProfile.InsuranceCalculationProfile));
    }

    private sealed class SalaryDecreeByIdDbResult
    {
        public Guid EmployeeId { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public decimal BaseDailySalary { get; set; }
        public decimal? AttractionAllowance { get; set; }
        public decimal? SupervisionAllowance { get; set; }
        public string ShiftType { get; set; } = null!;
        public string ContractType { get; set; } = null!;
        public decimal? HousingAllowance { get; set; }
        public decimal? FoodAllowance { get; set; }
        public decimal? TransportationAllowanceNet { get; set; }
        public decimal? KaranehAmountNet { get; set; }
        public string MaritalStatus { get; set; } = null!;
        public int ChildrenCount { get; set; }
        public bool IsTaxSubject { get; set; }
        public string InsuranceNumber { get; set; } = null!;
        public string? SocialSecurityContractRow { get; set; }
        public string PositionInInsuranceList { get; set; } = null!;
        public bool IsSubjectTo7PercentInsurance { get; set; }
        public bool IsSubjectTo20PercentInsurance { get; set; }
        public bool IsSubjectTo3PercentInsurance { get; set; }
        public bool IsSubjectTo4PercentInsurance { get; set; }
        public string InsuranceCalculationProfile { get; set; } = null!;
    }
}
