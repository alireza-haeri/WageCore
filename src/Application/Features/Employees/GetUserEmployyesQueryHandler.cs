using Core.Contracts;

namespace Application.Features.Employees;

public class GetUserEmployyesQueryHandler(IEmployeeQuery employeeQuery)
    : IRequestHandler<GetUserEmployyesQuery, Result<PagedResult<GetUserEmployyesQueryResponse>>>
{
    public async Task<Result<PagedResult<GetUserEmployyesQueryResponse>>> Handle(GetUserEmployyesQuery request,
        CancellationToken cancellationToken)
    {
        var userEmployeesPaged = await employeeQuery.GetUserEmployyesAsync(
            request.UserId,
            request.Pagination,
            request.Search,
            request.WorkshopId,
            request.DepartmentId,
            request.Status,
            cancellationToken);

        var response = userEmployeesPaged.Map(x =>
            new GetUserEmployyesQueryResponse(
                x.EmployeeId,
                x.PersonalCode,
                x.FullName,
                x.WorkshopName,
                x.DepartmentName,
                x.NationalCode,
                x.HireDate,
                x.JobTitle,
                x.Status)
        );

        return Result<PagedResult<GetUserEmployyesQueryResponse>>.Success(response);
    }
}
