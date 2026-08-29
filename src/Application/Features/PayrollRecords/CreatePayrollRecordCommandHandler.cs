namespace Application.Features.PayrollRecords;

public class CreatePayrollRecordCommandHandler(
    IEmployeeRepository employeeRepository,
    IPersianCalendarService persianCalendarService,
    IPayrollRecordQuery payrollRecordQuery,
    IPayrollCalculationService payrollCalculationService,
    IPayrollRecordRepository payrollRecordRepository)
    : IRequestHandler<CreatePayrollRecordCommand, Result<CreatePayrollRecordCommandResponse>>
{
    public async Task<Result<CreatePayrollRecordCommandResponse>> Handle(
        CreatePayrollRecordCommand request,
        CancellationToken cancellationToken)
    {
        var period = persianCalendarService.GetMonthRange(request.PersianYear, request.PersianMonth);

        if (period.StartPeriod > DateOnly.FromDateTime(DateTime.Now))
            return Result<CreatePayrollRecordCommandResponse>.GeneralFailure("تاریخ شروع دوره نباید برای آینده باشد.");

        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<CreatePayrollRecordCommandResponse>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var hasOverlappingPeriod = await payrollRecordQuery.HasOverlappingPeriodAsync(
            request.UserId,
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            null,
            cancellationToken);
        if (hasOverlappingPeriod)
            return Result<CreatePayrollRecordCommandResponse>.GeneralFailure(
                "برای این کارمند در این بازه فیش پرداختی دیگری ثبت شده است.");

        var calculationResult = await payrollCalculationService.CalculateAsync(
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            request.Work,
            cancellationToken);
        if (!calculationResult.IsSuccess)
            return Result<CreatePayrollRecordCommandResponse>.ValidationFailure(calculationResult.Errors!);

        var calculation = calculationResult.Response!;
        var payrollRecord = PayrollRecord.Create(
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            employee.IsTaxSubject,
            calculation.MaxMonthlyOvertimeHours,
            calculation.MaxFridayHours,
            request.Work,
            new PayrollRecordAmountsDto(
                calculation.OvertimeAmount,
                calculation.NightShiftExtraAmount,
                calculation.FridayWorkAllowance,
                calculation.CalculatedTaxAmount,
                calculation.NetPayableAmount));
        if (!payrollRecord.IsSuccess)
            return Result<CreatePayrollRecordCommandResponse>.GeneralFailure(payrollRecord.ErrorMessage!);

        var payrollRecordId = await payrollRecordRepository.CreateAsync(payrollRecord.Response!, cancellationToken);
        if (payrollRecordId is null)
            return Result<CreatePayrollRecordCommandResponse>.GeneralFailure("خطا در ایجاد فیش پرداختی");

        return Result<CreatePayrollRecordCommandResponse>.Success(
            new CreatePayrollRecordCommandResponse(payrollRecordId.Value));
    }
}
