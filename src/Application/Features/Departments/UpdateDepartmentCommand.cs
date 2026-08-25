namespace Application.Features.Departments;

public record UpdateDepartmentCommand(
    Guid UserId,
    Guid DepartmentId,
    string Name)
    : IRequest<Result<bool>>;
