namespace Application.Features.Employees;

public record DeleteEmployeeCommand(Guid UserId, Guid EmployeeId)
    : IRequest<Result<bool>>;
