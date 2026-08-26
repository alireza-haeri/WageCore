using Core.Abstractions.Repositories.Employees;

namespace Application.Features.Employees;

public class TerminateEmployeeCommandHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<TerminateEmployeeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(TerminateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<bool>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var domainResult = employee.Terminate(request.TerminationDate);
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await employeeRepository.UpdateAsync(employee, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در ثبت ترک کار کارمند رخ داد.");

        return Result<bool>.Success(true);
    }
}
