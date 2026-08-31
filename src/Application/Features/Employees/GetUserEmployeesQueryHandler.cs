using Core.Contracts;

namespace Application.Features.Employees;

public class GetUserEmployeesQueryHandler(IEmployeeQuery employeeQuery)
    : IRequestHandler<GetUserEmployeesQuery, Result<PagedResult<GetUserEmployeesQueryResponse>>>
{
    public async Task<Result<PagedResult<GetUserEmployeesQueryResponse>>> Handle(GetUserEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var userEmployeesPaged = await employeeQuery.GetUserEmployeesAsync(
            request.UserId,
            request.Pagination,
            request.Search,
            request.WorkshopId,
            request.DepartmentId,
            request.Status,
            cancellationToken);

        var response = userEmployeesPaged.Map(x =>
            new GetUserEmployeesQueryResponse(
                x.EmployeeId,
                x.PersonalCode,
                x.FullName,
                x.WorkshopName,
                x.DepartmentName,
                x.NationalCode,
                x.HireDate,
                x.JobTitle,
                x.Status,
                x.Region)
        );

        return Result<PagedResult<GetUserEmployeesQueryResponse>>.Success(response);
    }
}
