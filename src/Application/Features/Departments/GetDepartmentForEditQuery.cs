namespace Application.Features.Departments;

public record GetDepartmentForEditQuery(Guid UserId, Guid DepartmentId)
    : IRequest<Result<GetDepartmentForEditQueryResponse>>;

public record GetDepartmentForEditQueryResponse(
    string Name,
    Guid WorkshopId);
