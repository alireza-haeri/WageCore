namespace Application.Features.PayrollRecords;

public class MarkPayrollRecordAsPaidCommandHandler(IPayrollRecordRepository payrollRecordRepository)
    : IRequestHandler<MarkPayrollRecordAsPaidCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        MarkPayrollRecordAsPaidCommand request,
        CancellationToken cancellationToken)
    {
        var payrollRecord = await payrollRecordRepository.GetByIdAsync(
            request.UserId,
            request.PayrollRecordId,
            cancellationToken);
        if (payrollRecord is null || payrollRecord.EmployeeId != request.EmployeeId)
            return Result<bool>.NotfoundFailure("فیش پرداختی مورد نظر یافت نشد.");

        var markAsPaidResult = payrollRecord.MarkAsPaid();
        if (!markAsPaidResult.IsSuccess)
            return Result<bool>.GeneralFailure(markAsPaidResult.ErrorMessage!);

        var isUpdated = await payrollRecordRepository.UpdateAsync(payrollRecord, cancellationToken);
        if (!isUpdated)
            return Result<bool>.GeneralFailure("خطا در بروزرسانی فیش پرداختی");

        return Result<bool>.Success(true);
    }
}
