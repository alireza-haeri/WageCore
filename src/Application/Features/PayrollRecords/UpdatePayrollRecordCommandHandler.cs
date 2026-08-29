namespace Application.Features.PayrollRecords;

public class UpdatePayrollRecordCommandHandler(
    IEmployeeRepository employeeRepository,
    IPersianCalendarService persianCalendarService,
    IPayrollLimitsResolver payrollLimitsResolver,
    IPayrollRecordQuery payrollRecordQuery,
    IPayrollRecordRepository payrollRecordRepository,
    IWorkShopRepository workShopRepository,
    IEmployeeSalaryProfileQuery employeeSalaryProfileQuery,
    IPayrollCalculationService payrollCalculationService)
    : IRequestHandler<UpdatePayrollRecordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdatePayrollRecordCommand request,
        CancellationToken cancellationToken)
    {
        var period = persianCalendarService.GetMonthRange(request.PersianYear, request.PersianMonth);

        if (period.StartPeriod > DateOnly.FromDateTime(DateTime.Now))
            return Result<bool>.GeneralFailure("تاریخ شروع دوره نباید برای آینده باشد.");

        var payrollRecord = await payrollRecordRepository.GetByIdAsync(
            request.UserId,
            request.PayrollRecordId,
            cancellationToken);
        if (payrollRecord is null || payrollRecord.EmployeeId != request.EmployeeId)
            return Result<bool>.NotfoundFailure("فیش پرداختی مورد نظر یافت نشد.");

        var canModifyResult = payrollRecord.EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return Result<bool>.GeneralFailure(canModifyResult.ErrorMessage!);

        var limitsResult = await payrollLimitsResolver.ResolveAsync(
            period.StartPeriod,
            period.EndPeriod,
            cancellationToken);
        if (!limitsResult.IsSuccess)
            return Result<bool>.ValidationFailure(limitsResult.Errors!);

        var limits = limitsResult.Response!;

        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<bool>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var employmentResult = employee.EnsureEmployedDuring(period.StartPeriod, period.EndPeriod);
        if (!employmentResult.IsSuccess)
            return Result<bool>.GeneralFailure(employmentResult.ErrorMessage!);

        var hasOverlappingPeriod = await payrollRecordQuery.HasOverlappingPeriodAsync(
            request.UserId,
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            request.PayrollRecordId,
            cancellationToken);
        if (hasOverlappingPeriod)
            return Result<bool>.GeneralFailure("برای این کارمند در این بازه فیش پرداختی دیگری ثبت شده است.");

        var workshopTask = workShopRepository.GetByIdAsync(request.UserId, employee.WorkshopId, cancellationToken);
        var salaryProfilesTask = employeeSalaryProfileQuery.GetEmployeeSalaryProfilesAffectingPeriodAsync(
            request.UserId,
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            cancellationToken);

        await Task.WhenAll(workshopTask, salaryProfilesTask);

        var workshop = await workshopTask;
        var salaryProfiles = await salaryProfilesTask;

        if (workshop is null)
            return Result<bool>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        if (salaryProfiles.Count == 0)
            return Result<bool>.NotfoundFailure("برای این بازه حکم حقوقی کارمند یافت نشد.");

        var calculation = payrollCalculationService.Calculate(
            employee,
            workshop,
            salaryProfiles,
            period.StartPeriod,
            period.EndPeriod,
            request.Work);
        var updateResult = payrollRecord.Update(
            period.StartPeriod,
            period.EndPeriod,
            employee.IsTaxSubject,
            limits.MaxMonthlyOvertimeHours,
            limits.MaxFridayHours,
            request.Work,
            new PayrollRecordAmountsDto(
                calculation.OvertimeAmount,
                calculation.NightShiftExtraAmount,
                calculation.FridayWorkAllowance,
                calculation.CalculatedTaxAmount,
                calculation.NetPayableAmount));
        if (!updateResult.IsSuccess)
            return Result<bool>.GeneralFailure(updateResult.ErrorMessage!);

        var isUpdated = await payrollRecordRepository.UpdateAsync(payrollRecord, cancellationToken);
        if (!isUpdated)
            return Result<bool>.GeneralFailure("خطا در بروزرسانی فیش پرداختی");

        return Result<bool>.Success(true);
    }
}
