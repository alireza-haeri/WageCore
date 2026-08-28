using Microsoft.Extensions.Logging;

namespace Application.Features.EmployeeSalaryProfiles;

public class UpdateEmployeeSalaryProfileCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeSalaryProfileRepository employeeSalaryProfileRepository,
    IEmployeeSalaryProfileQuery employeeSalaryProfileQuery,
    IPayrollRecordQuery payrollRecordQuery,
    ILaborLawRuleQuery laborLawRuleQuery,
    ILogger<UpdateEmployeeSalaryProfileCommandHandler> logger)
    : IRequestHandler<UpdateEmployeeSalaryProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateEmployeeSalaryProfileCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<bool>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var salaryProfile = await employeeSalaryProfileRepository.GetByIdAsync(
            request.UserId,
            request.EmployeeSalaryProfileId,
            cancellationToken);
        if (salaryProfile is null || salaryProfile.EmployeeId != request.EmployeeId)
            return Result<bool>.NotfoundFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.");

        var hasPayrollRecordEffectOld = await payrollRecordQuery.HasPayrollRecordEffectAsync(
            request.UserId,
            request.EmployeeId,
            salaryProfile.EffectiveFrom,
            cancellationToken);
        if (hasPayrollRecordEffectOld)
            return Result<bool>.GeneralFailure("امکان ویرایش این حکم وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.");

        if (request.SalaryProfile.EffectiveFrom != salaryProfile.EffectiveFrom)
        {
            var hasPayrollRecordEffectNew = await payrollRecordQuery.HasPayrollRecordEffectAsync(
                request.UserId,
                request.EmployeeId,
                request.SalaryProfile.EffectiveFrom!.Value,
                cancellationToken);
            if (hasPayrollRecordEffectNew)
                return Result<bool>.GeneralFailure("امکان انتقال این حکم به این بازه وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.");
        }

        var minimumMonthlySalary = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MinimumMonthlySalary,
            request.SalaryProfile.EffectiveFrom!.Value,
            cancellationToken);

        if (minimumMonthlySalary is null)
        {
            logger.LogCritical("MinimumMonthlySalary for {DateTime} not found", request.SalaryProfile.EffectiveFrom);
            return Result<bool>.NotfoundFailure("حداقل حقوق ماهانه یافت نشد.");
        }

        var latestExistingEffectiveFrom = await employeeSalaryProfileQuery.GetLatestEffectiveFromAsync(
            request.UserId,
            request.EmployeeId,
            request.EmployeeSalaryProfileId,
            cancellationToken);

        var domainResult = salaryProfile.Update(
            employee.HireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            request.SalaryProfile);

        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await employeeSalaryProfileRepository.UpdateAsync(salaryProfile, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطا در بروزرسانی پروفایل حقوق کارمند");

        return Result<bool>.Success(true);
    }
}
