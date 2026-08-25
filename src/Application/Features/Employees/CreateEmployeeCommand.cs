using Core.Contracts.Employees;

namespace Application.Features.Employees;

public record CreateEmployeeCommand(
    Guid UserId,
    Guid WorkshopId,
    EmployeeDto Employee,
    EmployeeInsuranceDto Insurance)
    : IRequest<Result<CreateEmployeeCommandResponse>>;

public record CreateEmployeeCommandResponse(Guid EmployeeId);
