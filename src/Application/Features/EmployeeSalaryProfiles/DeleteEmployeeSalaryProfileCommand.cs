namespace Application.Features.EmployeeSalaryProfiles;

public record DeleteEmployeeSalaryProfileCommand(
    Guid UserId,
    Guid EmployeeSalaryProfileId)
    : IRequest<Result<bool>>;
