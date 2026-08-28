using Core.Contracts;

namespace Application.Features.EmployeeSalaryProfiles;

public record GetEmployeeSalaryProfilesQuery(
    Guid UserId,
    PaginationDto Pagination,
    Guid? EmployeeId = null,
    string? Search = null,
    EmployeeSalaryProfileStatus? Status = null,
    Guid? WorkshopId = null,
    Guid? DepartmentId = null)
    : IRequest<Result<PagedResult<GetEmployeeSalaryProfilesQueryResponse>>>;

public record GetEmployeeSalaryProfilesQueryResponse(
    Guid EmployeeSalaryProfileId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    DateOnly EffectiveFrom,
    decimal BaseMonthlySalary,
    EmployeeSalaryProfileStatus Status);
