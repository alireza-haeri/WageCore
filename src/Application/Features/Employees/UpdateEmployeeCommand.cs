using Core.Contracts.Employees;

namespace Application.Features.Employees;

public record UpdateEmployeeCommand(
    Guid UserId,
    Guid EmployeeId,
    EmployeeDto Employee)
    : IRequest<Result<bool>>;
