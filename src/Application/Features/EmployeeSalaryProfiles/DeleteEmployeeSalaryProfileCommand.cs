namespace Application.Features.EmployeeSalaryProfiles;

public record DeleteEmployeeSalaryProfileCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid EmployeeSalaryProfileId)
    : IRequest<Result<bool>>;
