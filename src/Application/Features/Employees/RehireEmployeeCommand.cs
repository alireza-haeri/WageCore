namespace Application.Features.Employees;

public record RehireEmployeeCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid DepartmentId,
    DateOnly? HireDate)
    : IRequest<Result<bool>>;
