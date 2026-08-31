using Microsoft.Extensions.Logging;

namespace Application.Features.SalaryDecrees;

public class CreateSalaryDecreeCommandHandler(
    IEmployeeRepository employeeRepository,
    ISalaryDecreeRepository salaryDecreeRepository,
    ISalaryDecreeQuery salaryDecreeQuery,
    ILaborLawRuleQuery laborLawRuleQuery,
    IPayrollRecordQuery payrollRecordQuery,
    ILogger<CreateSalaryDecreeCommandHandler> logger)
    : IRequestHandler<CreateSalaryDecreeCommand, Result<CreateSalaryDecreeCommandResponse>>
{
    public async Task<Result<CreateSalaryDecreeCommandResponse>> Handle(
        CreateSalaryDecreeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<CreateSalaryDecreeCommandResponse>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var latestExistingEffectiveFrom = await salaryDecreeQuery.GetLatestEffectiveFromAsync(
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
            return Result<CreateSalaryDecreeCommandResponse>.NotfoundFailure("حداقل حقوق ماهانه یافت نشد.");
        }

        var salaryProfile = SalaryDecree.Create(
            request.EmployeeId,
            employee.HireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            request.SalaryProfile);
        if (!salaryProfile.IsSuccess)
            return Result<CreateSalaryDecreeCommandResponse>.GeneralFailure(salaryProfile.ErrorMessage!);

        var hasPayrollRecordEffectNew = await payrollRecordQuery.HasPayrollRecordEffectAsync(
            request.UserId,
            request.EmployeeId,
            request.SalaryProfile.EffectiveFrom!.Value,
            cancellationToken);
        if (hasPayrollRecordEffectNew)
            return Result<CreateSalaryDecreeCommandResponse>.GeneralFailure("امکان انتقال این حکم به این بازه وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.");

        var createResult = await salaryDecreeRepository.CreateAsync(
            salaryProfile.Response!,
            cancellationToken);

        if (createResult is null)
            return Result<CreateSalaryDecreeCommandResponse>.GeneralFailure(
                "خطا در ایجاد پروفایل حقوق کارمند");

        return Result<CreateSalaryDecreeCommandResponse>.Success(
            new CreateSalaryDecreeCommandResponse(createResult.Value));
    }
}
