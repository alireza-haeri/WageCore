namespace Application.Features.Employees;

public record TerminateEmployeeCommand(
    Guid UserId,
    Guid EmployeeId,
    DateOnly? TerminationDate)
    : IRequest<Result<bool>>;
