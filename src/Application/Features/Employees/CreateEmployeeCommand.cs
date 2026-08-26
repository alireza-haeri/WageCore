using Core.Contracts.Employees;

namespace Application.Features.Employees;

public record CreateEmployeeCommand(
    Guid UserId,
    Guid WorkshopId,
    EmployeeDto Employee,
    EmployeeInsuranceDto Insurance,
    List<EmployeeBankAccountDto>? BankAccounts = null)
    : IRequest<Result<CreateEmployeeCommandResponse>>;

public record CreateEmployeeCommandResponse(Guid EmployeeId);
