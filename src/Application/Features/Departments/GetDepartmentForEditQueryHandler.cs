using Core.Abstractions.Repositories.Departments;

namespace Application.Features.Departments;

public class GetDepartmentForEditQueryHandler(IDepartmentQuery departmentQuery)
    : IRequestHandler<GetDepartmentForEditQuery, Result<GetDepartmentForEditQueryResponse>>
{
    public async Task<Result<GetDepartmentForEditQueryResponse>> Handle(GetDepartmentForEditQuery request,
        CancellationToken cancellationToken)
    {
        var department = await departmentQuery.GetUserDepartmentByIdAsync(request.UserId, request.DepartmentId, cancellationToken);
        if (department is null)
            return Result<GetDepartmentForEditQueryResponse>.NotfoundFailure("دپارتمان مورد نظر یافت نشد.");

        return Result<GetDepartmentForEditQueryResponse>.Success(
            new GetDepartmentForEditQueryResponse(
                department.Name,
                department.WorkshopId
            )
        );
    }
}
