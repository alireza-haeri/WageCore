namespace Application.Features.PayrollRecords;

public class DeletePayrollRecordCommandHandler(IPayrollRecordRepository payrollRecordRepository)
    : IRequestHandler<DeletePayrollRecordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeletePayrollRecordCommand request,
        CancellationToken cancellationToken)
    {
        var payrollRecord = await payrollRecordRepository.GetByIdAsync(
            request.UserId,
            request.PayrollRecordId,
            cancellationToken);
        if (payrollRecord is null || payrollRecord.EmployeeId != request.EmployeeId)
            return Result<bool>.NotfoundFailure("فیش پرداختی مورد نظر یافت نشد.");

        var canDeleteResult = payrollRecord.EnsureCanDelete();
        if (!canDeleteResult.IsSuccess)
            return Result<bool>.GeneralFailure(canDeleteResult.ErrorMessage!);

        var isDeleted = await payrollRecordRepository.DeleteAsync(
            request.UserId,
            request.PayrollRecordId,
            cancellationToken);
        if (!isDeleted)
            return Result<bool>.GeneralFailure("خطا در حذف فیش پرداختی");

        return Result<bool>.Success(true);
    }
}
