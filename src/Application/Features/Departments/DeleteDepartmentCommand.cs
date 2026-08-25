namespace Application.Features.Departments;

public record DeleteDepartmentCommand(Guid UserId, Guid DepartmentId)
    : IRequest<Result<bool>>;
