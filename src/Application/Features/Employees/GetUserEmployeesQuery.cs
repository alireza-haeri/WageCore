using Core.Contracts;

namespace Application.Features.Employees;

public record GetUserEmployeesQuery(
    Guid UserId,
    PaginationDto Pagination,
    string? Search = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null,
    EmployeeStatus? Status = null)
    : IRequest<Result<PagedResult<GetUserEmployeesQueryResponse>>>;

public record GetUserEmployeesQueryResponse(
    Guid Id,
    string PersonalCode,
    string FullName,
    string WorkshopName,
    string DepartmentName,
    string NationalCode,
    DateOnly HireDate,
    string? JobTitle,
    EmployeeStatus Status,
    Region Region);
