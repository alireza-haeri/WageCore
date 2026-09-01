using Core.Contracts;

namespace Application.Features.SalaryDecrees;

public record GetSalaryDecreesQuery(
    Guid UserId,
    PaginationDto Pagination,
    Guid? EmployeeId = null,
    string? Search = null,
    SalaryDecreeStatus? Status = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null)
    : IRequest<Result<PagedResult<GetSalaryDecreesQueryResponse>>>;

public record GetSalaryDecreesQueryResponse(
    Guid SalaryDecreeId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    DateOnly EffectiveFrom,
    decimal BaseDailySalary,
    SalaryDecreeStatus Status);
