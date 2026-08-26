using Core.Abstractions.Repositories.Employees;
using Core.Contracts.Employees;

namespace Application.Features.Employees;

public class RehireEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IWorkShopRepository workshopRepository)
    : IRequestHandler<RehireEmployeeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RehireEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<bool>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var workshop = await workshopRepository.GetByIdAsync(request.UserId, employee.WorkshopId, cancellationToken);
        if (workshop is null)
            return Result<bool>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        var departmentWorkshop = await workshopRepository.GetByDepartmentIdAsync(
            request.UserId,
            request.DepartmentId,
            cancellationToken);

        if (departmentWorkshop is null || departmentWorkshop.Id != employee.WorkshopId)
            return Result<bool>.NotfoundFailure("بخش مورد نظر یافت نشد.");

        var domainResult = employee.Rehire(new EmployeeRehireDto(
            request.DepartmentId,
            workshop.RegistrationDate,
            request.HireDate));

        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await employeeRepository.UpdateAsync(employee, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در استخدام مجدد کارمند رخ داد.");

        return Result<bool>.Success(true);
    }
}
