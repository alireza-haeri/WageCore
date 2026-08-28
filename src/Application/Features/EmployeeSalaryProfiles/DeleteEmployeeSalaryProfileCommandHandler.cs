namespace Application.Features.EmployeeSalaryProfiles;

public class DeleteEmployeeSalaryProfileCommandHandler(
    IEmployeeSalaryProfileRepository employeeSalaryProfileRepository,
    IPayrollRecordQuery payrollRecordQuery)
    : IRequestHandler<DeleteEmployeeSalaryProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteEmployeeSalaryProfileCommand request,
        CancellationToken cancellationToken)
    {
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

        var deleteResult = await employeeSalaryProfileRepository.DeleteAsync(
            request.UserId,
            request.EmployeeSalaryProfileId,
            cancellationToken);
        if (!deleteResult)
            return Result<bool>.GeneralFailure("خطا در حذف پروفایل حقوق کارمند");

        return Result<bool>.Success(true);
    }
}
