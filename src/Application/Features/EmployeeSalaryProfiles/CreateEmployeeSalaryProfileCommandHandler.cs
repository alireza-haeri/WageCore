using Core.Abstractions.Repositories.Employees;

namespace Application.Features.EmployeeSalaryProfiles;

public class CreateEmployeeSalaryProfileCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeSalaryProfileRepository employeeSalaryProfileRepository,
    IEmployeeSalaryProfileQuery employeeSalaryProfileQuery)
    : IRequestHandler<CreateEmployeeSalaryProfileCommand, Result<CreateEmployeeSalaryProfileCommandResponse>>
{
    public const decimal MinimumMonthlySalary = 71_661_840m;

    public async Task<Result<CreateEmployeeSalaryProfileCommandResponse>> Handle(
        CreateEmployeeSalaryProfileCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<CreateEmployeeSalaryProfileCommandResponse>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var latestExistingEffectiveFrom = await employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
            request.UserId,
            request.EmployeeId,
            cancellationToken);

        var salaryProfile = EmployeeSalaryProfile.Create(
            request.EmployeeId,
            employee.HireDate,
            latestExistingEffectiveFrom,
            MinimumMonthlySalary,
            request.SalaryProfile);

        if (!salaryProfile.IsSuccess)
            return Result<CreateEmployeeSalaryProfileCommandResponse>.GeneralFailure(salaryProfile.ErrorMessage!);

        var createResult = await employeeSalaryProfileRepository.CreateAsync(
            salaryProfile.Response!,
            cancellationToken);

        if (createResult is null)
            return Result<CreateEmployeeSalaryProfileCommandResponse>.GeneralFailure("خطا در ایجاد پروفایل حقوق کارمند");

        return Result<CreateEmployeeSalaryProfileCommandResponse>.Success(
            new CreateEmployeeSalaryProfileCommandResponse(createResult.Value));
    }
}
