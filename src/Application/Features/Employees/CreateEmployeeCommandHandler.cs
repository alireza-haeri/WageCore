using Core.Abstractions.Repositories.Employees;
using Core.Abstractions.Services;
using Core.Contracts.Employees;

namespace Application.Features.Employees;

public class CreateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeQuery employeeQuery,
    IWorkShopRepository workshopRepository,
    IPersianCalendarService persianCalendarService)
    : IRequestHandler<CreateEmployeeCommand, Result<CreateEmployeeCommandResponse>>
{
    public async Task<Result<CreateEmployeeCommandResponse>> Handle(CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = await workshopRepository.GetByIdAsync(request.UserId, request.WorkshopId, cancellationToken);
        if (workshop is null)
            return Result<CreateEmployeeCommandResponse>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        var departmentWorkshop = await workshopRepository.GetByDepartmentIdAsync(
            request.UserId,
            request.Employee.DepartmentId,
            cancellationToken);

        if (departmentWorkshop is null || departmentWorkshop.Id != request.WorkshopId)
            return Result<CreateEmployeeCommandResponse>.NotfoundFailure("بخش مورد نظر یافت نشد.");

        var existEmployeePersonalCode = await employeeQuery.IsExistEmployeePersonalCode(
            request.UserId,
            request.Employee.PersonalCode,
            null,
            cancellationToken);

        if (existEmployeePersonalCode)
            return Result<CreateEmployeeCommandResponse>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(EmployeeDto.PersonalCode), ["کد پرسنلی در بین کارکنان این کاربر تکراری است"] }
            });

        var existEmployeeNationalCode = await employeeQuery.IsExistEmployeeNationalCode(
            request.UserId,
            request.Employee.NationalCode,
            null,
            cancellationToken);

        if (existEmployeeNationalCode)
            return Result<CreateEmployeeCommandResponse>.ValidationFailure(new Dictionary<string, string[]>()
            {
                { nameof(EmployeeDto.NationalCode), ["کد ملی در بین کارکنان این کاربر تکراری است"] }
            });

        var employee = Employee.Create(
            request.WorkshopId,
            workshop.RegistrationDate,
            request.Employee,
            true,
            true,
            persianCalendarService);

        if (!employee.IsSuccess)
            return Result<CreateEmployeeCommandResponse>.GeneralFailure(employee.ErrorMessage!);

        var bankAccountsResult = employee.Response!.ReplaceBankAccounts(request.BankAccounts);
        if (!bankAccountsResult.IsSuccess)
            return Result<CreateEmployeeCommandResponse>.GeneralFailure(bankAccountsResult.ErrorMessage!);

        var createResult = await employeeRepository.CreateAsync(employee.Response, cancellationToken);
        if (createResult is null)
            return Result<CreateEmployeeCommandResponse>.GeneralFailure("خطا در ایجاد کارمند");

        return Result<CreateEmployeeCommandResponse>.Success(new CreateEmployeeCommandResponse(createResult.Value));
    }
}
