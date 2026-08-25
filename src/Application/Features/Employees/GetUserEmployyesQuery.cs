using Core.Contracts;

namespace Application.Features.Employees;

public record GetUserEmployyesQuery(
    Guid UserId,
    PaginationDto Pagination,
    string? Search = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null,
    EmployeeStatus? Status = null)
    : IRequest<Result<PagedResult<GetUserEmployyesQueryResponse>>>;

public record GetUserEmployyesQueryResponse(
    Guid Id,
    string PersonalCode,
    string FullName,
    string WorkshopName,
    string DepartmentName,
    string NationalCode,
    DateOnly HireDate,
    string? JobTitle,
    EmployeeStatus Status);
