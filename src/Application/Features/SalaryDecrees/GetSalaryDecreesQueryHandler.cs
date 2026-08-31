using Core.Contracts;

namespace Application.Features.SalaryDecrees;

public class GetSalaryDecreesQueryHandler(ISalaryDecreeQuery salaryDecreeQuery)
    : IRequestHandler<GetSalaryDecreesQuery, Result<PagedResult<GetSalaryDecreesQueryResponse>>>
{
    public async Task<Result<PagedResult<GetSalaryDecreesQueryResponse>>> Handle(
        GetSalaryDecreesQuery request,
        CancellationToken cancellationToken)
    {
        var pagedSalaryProfiles = await salaryDecreeQuery.GetSalaryDecreesAsync(
            request.UserId,
            request.Pagination,
            request.EmployeeId,
            request.Search,
            request.Status,
            request.WorkshopId,
            request.DepartmentId,
            cancellationToken);

        var response = pagedSalaryProfiles.Map(x =>
            new GetSalaryDecreesQueryResponse(
                x.SalaryDecreeId,
                x.EmployeeId,
                x.EmployeeName,
                x.PersonalCode,
                x.WorkshopName,
                x.DepartmentName,
                x.EffectiveFrom,
                x.BaseDailySalary,
                x.Status));

        return Result<PagedResult<GetSalaryDecreesQueryResponse>>.Success(response);
    }
}
