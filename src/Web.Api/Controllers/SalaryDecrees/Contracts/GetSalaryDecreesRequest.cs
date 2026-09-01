namespace Web.Api.Controllers.SalaryDecrees.Contracts;

public record GetSalaryDecreesRequest(
    PaginationDto Pagination,
    Guid? EmployeeId = null,
    string? Search = null,
    SalaryDecreeStatus? Status = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null);

public record GetSalaryDecreesResponse(
    Guid SalaryDecreeId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    string DisplayEffectiveFrom,
    decimal BaseDailySalary,
    SalaryDecreeStatus Status);
