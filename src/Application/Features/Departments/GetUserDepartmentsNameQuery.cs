namespace Application.Features.Departments;

public record GetUserDepartmentsNameQuery(Guid UserId)
    : IRequest<Result<GetUserDepartmentsNameQueryResponse>>;

public record GetUserDepartmentsNameQueryResponse(List<GetUserDepartmentsNameQueryResponseDepartmentName> DepartmentNames);

public record GetUserDepartmentsNameQueryResponseDepartmentName(Guid DepartmentId, string DisplayName);
