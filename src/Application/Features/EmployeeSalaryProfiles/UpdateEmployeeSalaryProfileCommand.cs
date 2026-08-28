namespace Application.Features.EmployeeSalaryProfiles;

public record UpdateEmployeeSalaryProfileCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid EmployeeSalaryProfileId,
    EmployeeSalaryProfileDto SalaryProfile)
    : IRequest<Result<bool>>;
