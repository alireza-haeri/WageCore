namespace Web.Api.Controllers.EmployeeSalaryProfiles.Contracts;

public record GetEmployeeSalaryProfilesRequest(
    PaginationDto Pagination,
    Guid? EmployeeId = null,
    string? Search = null,
    EmployeeSalaryProfileStatus? Status = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null);

public record GetEmployeeSalaryProfilesResponse(
    Guid EmployeeSalaryProfileId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    string DisplayEffectiveFrom,
    decimal BaseMonthlySalary,
    EmployeeSalaryProfileStatus Status);
