using Core.Contracts;

namespace Application.Features.Workshops;

public record GetUserWorkshopsQuery(
    Guid UserId,
    PaginationDto Pagination,
    string? SearchName = null)
    : IRequest<Result<PagedResult<GetUserWorkshopsQueryResponse>>>;

public record GetUserWorkshopsQueryResponse(
    Guid Id,
    string Name,
    string Address,
    string NationalId,
    DateOnly RegistrationDate,
    int EmployeesCount,
    int DepartmentsCount);