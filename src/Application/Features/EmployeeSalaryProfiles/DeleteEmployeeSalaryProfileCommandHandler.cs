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
        if (salaryProfile is null)
            return Result<bool>.NotfoundFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.");

        var hasPayrollRecordEffect = await payrollRecordQuery.HasPayrollRecordEffectAsync(
            request.UserId,
            salaryProfile.EmployeeId,
            salaryProfile.EffectiveFrom,
            cancellationToken);

        if (hasPayrollRecordEffect)
            return Result<bool>.GeneralFailure("این پروفایل حقوق بر روی فیش حقوقی اثر دارد و امکان حذف آن وجود ندارد.");

        var deleteResult = await employeeSalaryProfileRepository.DeleteAsync(
            request.UserId,
            request.EmployeeSalaryProfileId,
            cancellationToken);
        if (!deleteResult)
            return Result<bool>.GeneralFailure("خطا در حذف پروفایل حقوق کارمند");

        return Result<bool>.Success(true);
    }
}
