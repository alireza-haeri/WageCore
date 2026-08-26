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

    public async Task<UserEmployeeByIdResult?> GetUserEmployeeByIdAsync(Guid userId, Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT
                          e.WorkshopId AS WorkshopId,
                          e.DepartmentId AS DepartmentId,
                          e.PersonalCode AS PersonalCode,
                          e.FullName AS FullName,
                          e.NationalCode AS NationalCode,
                          e.BirthCertificateNumber AS BirthCertificateNumber,
                          e.FatherName AS FatherName,
                          e.Gender AS Gender,
                          e.MaritalStatus AS MaritalStatus,
                          e.ChildrenCount AS ChildrenCount,
                          e.HireDate AS HireDate,
                          e.PhoneNumber AS PhoneNumber,
                          e.JobTitle AS JobTitle,
                          e.IsTaxSubject AS IsTaxSubject,
                          e.InsuranceNumber AS InsuranceNumber,
                          e.SocialSecurityContractRow AS SocialSecurityContractRow,
                          e.PositionInInsuranceList AS PositionInInsuranceList,
                          e.IsSubjectTo7PercentInsurance AS IsSubjectTo7PercentInsurance,
                          e.IsSubjectTo20PercentInsurance AS IsSubjectTo20PercentInsurance,
                          e.IsSubjectTo3PercentInsurance AS IsSubjectTo3PercentInsurance,
                          e.InsuranceCalculationProfile AS InsuranceCalculationProfile
                      FROM {Core.Domain.Employee.TableName} e
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      WHERE w.UserId = @UserId AND e.Id = @EmployeeId;

                      SELECT
                          ba.Id AS Id,
                          ba.Title AS Title,
                          ba.Iban AS Iban
                      FROM {BankAccount.TableName} ba
                      INNER JOIN {Core.Domain.Employee.TableName} e ON e.Id = ba.EmployeeId
                      INNER JOIN {Core.Domain.Workshop.TableName} w ON w.Id = e.WorkshopId
                      WHERE w.UserId = @UserId AND ba.EmployeeId = @EmployeeId
                      ORDER BY ba.Id ASC;
                      """;

        var command = new CommandDefinition(sql, new { UserId = userId, EmployeeId = employeeId },
            cancellationToken: cancellationToken);

        using var connection = dbConnectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(command);

        var employee = await multi.ReadFirstOrDefaultAsync<UserEmployeeByIdDbResult>();
        if (employee is null)
            return null;

        var bankAccounts = (await multi.ReadAsync<EmployeeBankAccountDbResult>())
            .Select(x => new EmployeeBankAccountDto(x.Title, x.Iban, x.Id))
            .ToList();

        return new UserEmployeeByIdResult(
            employee.WorkshopId,
            employee.DepartmentId,
            employee.PersonalCode,
            employee.FullName,
            employee.NationalCode,
            employee.BirthCertificateNumber,
            employee.FatherName,
            Enum.Parse<EmployeeGender>(employee.Gender),
            Enum.Parse<EmployeeMaritalStatus>(employee.MaritalStatus),
            employee.ChildrenCount,
            DateOnly.FromDateTime(employee.HireDate),
            employee.PhoneNumber,
            employee.JobTitle,
            employee.IsTaxSubject,
            employee.InsuranceNumber,
            employee.SocialSecurityContractRow,
            employee.PositionInInsuranceList,
            employee.IsSubjectTo7PercentInsurance,
            employee.IsSubjectTo20PercentInsurance,
            employee.IsSubjectTo3PercentInsurance,
            Enum.Parse<InsuranceCalculationProfile>(employee.InsuranceCalculationProfile),
            bankAccounts);
    }

    private sealed class EmployeeBankAccountDbResult
    {
        public Guid? Id { get; set; }
        public string? Title { get; set; }
        public string Iban { get; set; } = null!;
    }

    private sealed class UserEmployeeByIdDbResult
    {
        public Guid WorkshopId { get; set; }
        public Guid DepartmentId { get; set; }
        public string PersonalCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string NationalCode { get; set; } = null!;
        public string BirthCertificateNumber { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string MaritalStatus { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public int ChildrenCount { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? JobTitle { get; set; }
        public bool IsTaxSubject { get; set; }
        public string InsuranceNumber { get; set; } = null!;
        public string? SocialSecurityContractRow { get; set; }
        public string PositionInInsuranceList { get; set; } = null!;
        public bool IsSubjectTo7PercentInsurance { get; set; }
        public bool IsSubjectTo20PercentInsurance { get; set; }
        public bool IsSubjectTo3PercentInsurance { get; set; }
        public string InsuranceCalculationProfile { get; set; } = null!;
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
