namespace Application.Features.SalaryDecrees;

public record UpdateSalaryDecreeCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid SalaryDecreeId,
    SalaryDecreeDto SalaryProfile)
    : IRequest<Result<bool>>;
