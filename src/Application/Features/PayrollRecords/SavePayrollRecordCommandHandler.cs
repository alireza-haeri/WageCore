namespace Application.Features.PayrollRecords;

/// <summary>
/// Creates a new payroll record or recalculates and updates the existing one
/// for the same employee and period. The identity of a payslip is
/// (employee, persian year, persian month); a record that has been marked as
/// paid can no longer be modified.
/// </summary>
public class SavePayrollRecordCommandHandler(
    IPersianCalendarService persianCalendarService,
    IPayrollLimitsResolver payrollLimitsResolver,
    IPayrollRecordRepository payrollRecordRepository,
    IEmployeeRepository employeeRepository,
    IWorkShopRepository workShopRepository,
    ISalaryDecreeQuery salaryDecreeQuery,
    IPayrollCalculationService payrollCalculationService)
    : IRequestHandler<SavePayrollRecordCommand, Result<SavePayrollRecordCommandResponse>>
{
    public async Task<Result<SavePayrollRecordCommandResponse>> Handle(
        SavePayrollRecordCommand request,
        CancellationToken cancellationToken)
    {
        var period = persianCalendarService.GetMonthRange(request.PersianYear, request.PersianMonth);

        if (period.StartPeriod > DateOnly.FromDateTime(DateTime.Now))
            return Result<SavePayrollRecordCommandResponse>.GeneralFailure("تاریخ شروع دوره نباید برای آینده باشد.");

        var standardWorkingDaysCount = period.EndPeriod.DayNumber - period.StartPeriod.DayNumber + 1;
        var isEsfandPeriod = request.PersianMonth == 12;

        var limitsResult = await payrollLimitsResolver.ResolveAsync(
            period.StartPeriod,
            period.EndPeriod,
            cancellationToken);
        if (!limitsResult.IsSuccess)
            return Result<SavePayrollRecordCommandResponse>.ValidationFailure(limitsResult.Errors!);

        var limits = limitsResult.Response!;

        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<SavePayrollRecordCommandResponse>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var employmentResult = employee.EnsureEmployedDuring(period.StartPeriod, period.EndPeriod);
        if (!employmentResult.IsSuccess)
            return Result<SavePayrollRecordCommandResponse>.GeneralFailure(employmentResult.ErrorMessage!);

        var existingPayrollRecord = await payrollRecordRepository.GetByEmployeeAndPeriodAsync(
            request.UserId,
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            cancellationToken);

        if (existingPayrollRecord is not null && existingPayrollRecord.Status == PayrollRecordStatus.Paid)
            return Result<SavePayrollRecordCommandResponse>.GeneralFailure(
                "این فیش پرداخت شده است و قابل ویرایش نیست.");

        var workshop = await workShopRepository.GetByIdAsync(request.UserId, employee.WorkshopId, cancellationToken);
        var salaryProfiles = await salaryDecreeQuery.GetSalaryDecreesAffectingPeriodAsync(
            request.UserId,
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            cancellationToken);

        if (workshop is null)
            return Result<SavePayrollRecordCommandResponse>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        if (salaryProfiles.Count == 0)
            return Result<SavePayrollRecordCommandResponse>.NotfoundFailure(
                "برای این بازه حکم حقوقی کارمند یافت نشد.");

        var workInput = PayrollWorkInputMapper.Map(
            request.Work,
            standardWorkingDaysCount,
            isEsfandPeriod,
            limits.DailyWorkingHours);

        var calculationResult = await payrollCalculationService.CalculateAsync(
            employee,
            workshop,
            salaryProfiles,
            period.StartPeriod,
            period.EndPeriod,
            workInput,
            cancellationToken);
        if (!calculationResult.IsSuccess)
            return Result<SavePayrollRecordCommandResponse>.ValidationFailure(calculationResult.Errors!);

        var calculation = calculationResult.Response!;

        if (existingPayrollRecord is not null)
        {
            var updateDomainResult = existingPayrollRecord.Update(
                period.StartPeriod,
                period.EndPeriod,
                salaryProfiles[0].IsTaxSubject,
                limits.MaxMonthlyOvertimeHours,
                limits.MaxFridayHours,
                limits.MaxNightShiftHours,
                limits.DailyWorkingHours,
                workInput,
                calculation.Amounts,
                calculation.CalculatedAmounts);
            if (!updateDomainResult.IsSuccess)
                return Result<SavePayrollRecordCommandResponse>.GeneralFailure(updateDomainResult.ErrorMessage!);

            var updateResult = await payrollRecordRepository.UpdateAsync(existingPayrollRecord, cancellationToken);
            if (!updateResult)
                return Result<SavePayrollRecordCommandResponse>.GeneralFailure("خطا در ویرایش فیش پرداختی");
            
            return Result<SavePayrollRecordCommandResponse>.Success(
                new SavePayrollRecordCommandResponse(existingPayrollRecord.Id));
        }

        var payrollRecord = PayrollRecord.Create(
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            salaryProfiles[0].IsTaxSubject,
            limits.MaxMonthlyOvertimeHours,
            limits.MaxFridayHours,
            limits.MaxNightShiftHours,
            limits.DailyWorkingHours,
            workInput,
            calculation.Amounts,
            calculation.CalculatedAmounts);
        if (!payrollRecord.IsSuccess)
            return Result<SavePayrollRecordCommandResponse>.GeneralFailure(payrollRecord.ErrorMessage!);

        var payrollRecordId = await payrollRecordRepository.CreateAsync(payrollRecord.Response!, cancellationToken);
        if (payrollRecordId is null)
            return Result<SavePayrollRecordCommandResponse>.GeneralFailure("خطا در ایجاد فیش پرداختی");

        return Result<SavePayrollRecordCommandResponse>.Success(
            new SavePayrollRecordCommandResponse(payrollRecordId.Value));
    }
}
