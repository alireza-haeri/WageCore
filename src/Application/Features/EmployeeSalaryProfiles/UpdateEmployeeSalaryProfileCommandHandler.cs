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
        if (salaryProfile is null)
            return Result<bool>.NotfoundFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.");

        if (salaryProfile.EmployeeId != request.EmployeeId)
            return Result<bool>.NotfoundFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.");

        var ruleDate = request.SalaryProfile.EffectiveFrom ?? salaryProfile.EffectiveFrom;

        var hasPayrollRecordEffect = await payrollRecordQuery.HasPayrollRecordEffectAsync(
            request.UserId,
            request.EmployeeId,
            ruleDate,
            cancellationToken);

        if (hasPayrollRecordEffect)
            return Result<bool>.GeneralFailure("این پروفایل حقوق بر روی فیش حقوقی اثر دارد و امکان ویرایش آن وجود ندارد.");

        var minimumMonthlySalary = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MinimumMonthlySalary,
            ruleDate,
            cancellationToken);

        if (minimumMonthlySalary is null)
        {
            logger.LogCritical("MinimumMonthlySalary for {DateTime} not found", ruleDate);
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
