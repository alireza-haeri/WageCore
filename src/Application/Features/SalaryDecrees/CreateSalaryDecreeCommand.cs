namespace Application.Features.SalaryDecrees;

public record CreateSalaryDecreeCommand(
    Guid UserId,
    Guid EmployeeId,
    SalaryDecreeDto SalaryProfile)
    : IRequest<Result<CreateSalaryDecreeCommandResponse>>;

public record CreateSalaryDecreeCommandResponse(Guid SalaryDecreeId);
