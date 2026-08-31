namespace Application.Features.SalaryDecrees;

public record DeleteSalaryDecreeCommand(
    Guid UserId,
    Guid EmployeeId,
    Guid SalaryDecreeId)
    : IRequest<Result<bool>>;
