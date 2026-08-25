using Core.Abstractions.Repositories.Departments;
using Core.Contracts;

namespace Application.Features.Departments;

public class GetUserDepartmentsQueryHandler(IDepartmentQuery departmentQuery)
    : IRequestHandler<GetUserDepartmentsQuery, Result<PagedResult<GetUserDepartmentsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetUserDepartmentsQueryResponse>>> Handle(GetUserDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var userDepartmentsPaged = await departmentQuery.GetUserDepartmentsAsync(
            request.UserId,
            request.Pagination,
            request.SearchName,
            request.WorkshopId,
            cancellationToken);

        var response = userDepartmentsPaged.Map(x =>
            new GetUserDepartmentsQueryResponse(
                x.DepartmentId, x.Name, x.WorkshopId, x.WorkshopName, x.EmployeesCount)
        );

        return Result<PagedResult<GetUserDepartmentsQueryResponse>>.Success(response);
    }
}
