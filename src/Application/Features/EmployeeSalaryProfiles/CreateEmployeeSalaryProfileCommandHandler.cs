using Microsoft.Extensions.Logging;

namespace Application.Features.EmployeeSalaryProfiles;

public class CreateEmployeeSalaryProfileCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeSalaryProfileRepository employeeSalaryProfileRepository,
    IEmployeeSalaryProfileQuery employeeSalaryProfileQuery,
    ILaborLawRuleQuery laborLawRuleQuery,
    IPayrollRecordQuery payrollRecordQuery,
    ILogger<CreateEmployeeSalaryProfileCommandHandler> logger)
    : IRequestHandler<CreateEmployeeSalaryProfileCommand, Result<CreateEmployeeSalaryProfileCommandResponse>>
{
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
            null,
            cancellationToken);

        var ruleDate = request.SalaryProfile.EffectiveFrom ?? DateOnly.FromDateTime(DateTime.Now);
        var minimumMonthlySalary = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.MinimumMonthlySalary,
            ruleDate,
            cancellationToken);

        if (minimumMonthlySalary is null)
        {
            logger.LogCritical("MinimumMonthlySalary for {DateTime} not found", ruleDate);
            return Result<CreateEmployeeSalaryProfileCommandResponse>.NotfoundFailure("حداقل حقوق ماهانه یافت نشد.");
        }

        var salaryProfile = EmployeeSalaryProfile.Create(
            request.EmployeeId,
            employee.HireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            request.SalaryProfile);
        if (!salaryProfile.IsSuccess)
            return Result<CreateEmployeeSalaryProfileCommandResponse>.GeneralFailure(salaryProfile.ErrorMessage!);

        var hasPayrollRecordEffectNew = await payrollRecordQuery.HasPayrollRecordEffectAsync(
            request.UserId,
            request.EmployeeId,
            request.SalaryProfile.EffectiveFrom!.Value,
            cancellationToken);
        if (hasPayrollRecordEffectNew)
            return Result<CreateEmployeeSalaryProfileCommandResponse>.GeneralFailure("امکان انتقال این حکم به این بازه وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.");
        
        var createResult = await employeeSalaryProfileRepository.CreateAsync(
            salaryProfile.Response!,
            cancellationToken);

        if (createResult is null)
            return Result<CreateEmployeeSalaryProfileCommandResponse>.GeneralFailure(
                "خطا در ایجاد پروفایل حقوق کارمند");

        return Result<CreateEmployeeSalaryProfileCommandResponse>.Success(
            new CreateEmployeeSalaryProfileCommandResponse(createResult.Value));
    }
}