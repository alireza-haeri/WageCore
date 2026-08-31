namespace Web.Api.Controllers.Employees.Contracts;

public record GetUserEmployeesRequest(
    PaginationDto Pagination,
    string? Search = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null,
    EmployeeStatus? Status = null
);

public record GetUserEmployeesResponse(
    Guid Id,
    string PersonalCode,
    string FullName,
    string WorkshopName,
    string DepartmentName,
    string NationalCode,
    string DisplayHireDate,
    string? JobTitle,
    EmployeeStatus Status,
    Region Region);
