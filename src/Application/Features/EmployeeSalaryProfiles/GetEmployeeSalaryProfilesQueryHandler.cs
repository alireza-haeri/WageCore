using Core.Contracts;

namespace Application.Features.EmployeeSalaryProfiles;

public class GetEmployeeSalaryProfilesQueryHandler(IEmployeeSalaryProfileQuery employeeSalaryProfileQuery)
    : IRequestHandler<GetEmployeeSalaryProfilesQuery, Result<PagedResult<GetEmployeeSalaryProfilesQueryResponse>>>
{
    public async Task<Result<PagedResult<GetEmployeeSalaryProfilesQueryResponse>>> Handle(
        GetEmployeeSalaryProfilesQuery request,
        CancellationToken cancellationToken)
    {
        var pagedSalaryProfiles = await employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAsync(
            request.UserId,
            request.Pagination,
            request.EmployeeId,
            request.Search,
            request.Status,
            request.WorkshopId,
            request.DepartmentId,
            cancellationToken);

        var response = pagedSalaryProfiles.Map(x =>
            new GetEmployeeSalaryProfilesQueryResponse(
                x.EmployeeSalaryProfileId,
                x.EmployeeId,
                x.EmployeeName,
                x.PersonalCode,
                x.WorkshopName,
                x.DepartmentName,
                x.EffectiveFrom,
                x.BaseMonthlySalary,
                x.Status));

        return Result<PagedResult<GetEmployeeSalaryProfilesQueryResponse>>.Success(response);
    }
}
