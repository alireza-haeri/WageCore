namespace Application.Features.EmployeeSalaryProfiles;

public record CreateEmployeeSalaryProfileCommand(
    Guid UserId,
    Guid EmployeeId,
    EmployeeSalaryProfileDto SalaryProfile)
    : IRequest<Result<CreateEmployeeSalaryProfileCommandResponse>>;

public record CreateEmployeeSalaryProfileCommandResponse(Guid EmployeeSalaryProfileId);
