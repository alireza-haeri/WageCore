namespace Application.Features.Departments;

public record CreateDepartmentCommand(
    Guid UserId,
    Guid WorkshopId,
    string Name)
    : IRequest<Result<CreateDepartmentCommandResponse>>;

public record CreateDepartmentCommandResponse(Guid DepartmentId);
