using Core.Contracts;

namespace Application.Features.Departments;

public record GetUserDepartmentsQuery(
    Guid UserId,
    PaginationDto Pagination,
    string? SearchName = null,
    Guid? WorkshopId = null)
    : IRequest<Result<PagedResult<GetUserDepartmentsQueryResponse>>>;

public record GetUserDepartmentsQueryResponse(
    Guid Id,
    string Name,
    Guid WorkshopId,
    string WorkshopName,
    int EmployeesCount);
