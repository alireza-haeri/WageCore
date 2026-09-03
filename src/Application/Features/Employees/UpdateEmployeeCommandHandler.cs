using Core.Abstractions.Repositories.Employees;
using Core.Abstractions.Services;
using Core.Contracts.Employees;

namespace Application.Features.Employees;

public class UpdateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeQuery employeeQuery,
    IWorkShopRepository workshopRepository,
    IPersianCalendarService persianCalendarService)
    : IRequestHandler<UpdateEmployeeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<bool>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var workshop = await workshopRepository.GetByIdAsync(request.UserId, employee.WorkshopId, cancellationToken);
        if (workshop is null)
            return Result<bool>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        var departmentWorkshop = await workshopRepository.GetByDepartmentIdAsync(
            request.UserId,
            request.Employee.DepartmentId,
            cancellationToken);

        if (departmentWorkshop is null || departmentWorkshop.Id != employee.WorkshopId)
            return Result<bool>.NotfoundFailure("بخش مورد نظر یافت نشد.");

        var existEmployeePersonalCode = await employeeQuery.IsExistEmployeePersonalCode(
            request.UserId,
            request.Employee.PersonalCode,
            request.EmployeeId,
            cancellationToken);

        if (existEmployeePersonalCode)
            return Result<bool>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(EmployeeDto.PersonalCode), ["کد پرسنلی در بین کارکنان این کاربر تکراری است"] }
            });

        var existEmployeeNationalCode = await employeeQuery.IsExistEmployeeNationalCode(
            request.UserId,
            request.Employee.NationalCode,
            request.EmployeeId,
            cancellationToken);

        if (existEmployeeNationalCode)
            return Result<bool>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(EmployeeDto.NationalCode), ["کد ملی در بین کارکنان این کاربر تکراری است"] }
            });

        var domainResult = employee.Update(
            request.Employee,
            workshop.RegistrationDate,
            true,
            true,
            persianCalendarService);
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var bankAccountsResult = employee.ReplaceBankAccounts(request.BankAccounts);
        if (!bankAccountsResult.IsSuccess)
            return Result<bool>.GeneralFailure(bankAccountsResult.ErrorMessage!);

        var updateResult = await employeeRepository.UpdateAsync(employee, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در بروزرسانی کارمند رخ داد.");

        return Result<bool>.Success(true);
    }
}
