namespace Application.Features.Departments;

public class GetUserDepartmentsNameQueryHandler(IDepartmentQuery departmentQuery)
    : IRequestHandler<GetUserDepartmentsNameQuery, Result<GetUserDepartmentsNameQueryResponse>>
{
    public async Task<Result<GetUserDepartmentsNameQueryResponse>> Handle(GetUserDepartmentsNameQuery request,
        CancellationToken cancellationToken)
    {
        var userDepartmentsName = await departmentQuery.GetUserDepartmentsNameAsync(request.UserId, cancellationToken);
        var response = userDepartmentsName
            .Select(u => new GetUserDepartmentsNameQueryResponseDepartmentName(u.DepartmentId, u.DisplayName))
            .ToList();

        return Result<GetUserDepartmentsNameQueryResponse>.Success(new GetUserDepartmentsNameQueryResponse(response));
    }
}
