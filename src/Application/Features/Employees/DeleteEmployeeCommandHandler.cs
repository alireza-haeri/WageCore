namespace Application.Features.Employees;

public class DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<DeleteEmployeeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var deleteResult = await employeeRepository.DeleteAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (!deleteResult)
            return Result<bool>.GeneralFailure("خطایی در حذف کارمند رخ داد.");

        return Result<bool>.Success(true);
    }
}
