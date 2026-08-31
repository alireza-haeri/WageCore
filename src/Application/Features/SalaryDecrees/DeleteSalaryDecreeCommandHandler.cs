namespace Application.Features.SalaryDecrees;

public class DeleteSalaryDecreeCommandHandler(
    ISalaryDecreeRepository salaryDecreeRepository,
    IPayrollRecordQuery payrollRecordQuery)
    : IRequestHandler<DeleteSalaryDecreeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteSalaryDecreeCommand request,
        CancellationToken cancellationToken)
    {
        var salaryProfile = await salaryDecreeRepository.GetByIdAsync(
            request.UserId,
            request.SalaryDecreeId,
            cancellationToken);
        if (salaryProfile is null || salaryProfile.EmployeeId != request.EmployeeId)
            return Result<bool>.NotfoundFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.");

        var hasPayrollRecordEffectOld = await payrollRecordQuery.HasPayrollRecordEffectAsync(
            request.UserId,
            request.EmployeeId,
            salaryProfile.EffectiveFrom,
            cancellationToken);
        if (hasPayrollRecordEffectOld)
            return Result<bool>.GeneralFailure("امکان حذف این حکم وجود ندارد، چون فیش پرداختی برای این بازه صادر شده است.");

        var deleteResult = await salaryDecreeRepository.DeleteAsync(
            request.UserId,
            request.SalaryDecreeId,
            cancellationToken);
        if (!deleteResult)
            return Result<bool>.GeneralFailure("خطا در حذف پروفایل حقوق کارمند");

        return Result<bool>.Success(true);
    }
}
