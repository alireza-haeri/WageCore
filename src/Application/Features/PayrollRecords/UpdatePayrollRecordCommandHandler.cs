namespace Application.Features.PayrollRecords;

public class UpdatePayrollRecordCommandHandler(
    IEmployeeRepository employeeRepository,
    IPersianCalendarService persianCalendarService,
    IPayrollLimitsResolver payrollLimitsResolver,
    IPayrollRecordQuery payrollRecordQuery,
    IPayrollRecordRepository payrollRecordRepository,
    IWorkShopRepository workShopRepository,
    ISalaryDecreeQuery salaryDecreeQuery,
    IPayrollCalculationService payrollCalculationService)
    : IRequestHandler<UpdatePayrollRecordCommand, Result<UpdatePayrollRecordCommandResponse>>
{
    public async Task<Result<UpdatePayrollRecordCommandResponse>> Handle(
        UpdatePayrollRecordCommand request,
        CancellationToken cancellationToken)
    {
        var period = persianCalendarService.GetMonthRange(request.PersianYear, request.PersianMonth);

        if (period.StartPeriod > DateOnly.FromDateTime(DateTime.Now))
            return Result<UpdatePayrollRecordCommandResponse>.GeneralFailure("تاریخ شروع دوره نباید برای آینده باشد.");

        var standardWorkingDaysCount = period.EndPeriod.DayNumber - period.StartPeriod.DayNumber + 1;
        var isEsfandPeriod = request.PersianMonth == 12;

        var payrollRecord = await payrollRecordRepository.GetByIdAsync(
            request.UserId,
            request.PayrollRecordId,
            cancellationToken);
        if (payrollRecord is null || payrollRecord.EmployeeId != request.EmployeeId)
            return Result<UpdatePayrollRecordCommandResponse>.NotfoundFailure("فیش پرداختی مورد نظر یافت نشد.");

        var canModifyResult = payrollRecord.EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return Result<UpdatePayrollRecordCommandResponse>.GeneralFailure(canModifyResult.ErrorMessage!);

        var limitsResult = await payrollLimitsResolver.ResolveAsync(
            period.StartPeriod,
            period.EndPeriod,
            cancellationToken);
        if (!limitsResult.IsSuccess)
            return Result<UpdatePayrollRecordCommandResponse>.ValidationFailure(limitsResult.Errors!);

        var limits = limitsResult.Response!;

        var employee = await employeeRepository.GetByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<UpdatePayrollRecordCommandResponse>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        var employmentResult = employee.EnsureEmployedDuring(period.StartPeriod, period.EndPeriod);
        if (!employmentResult.IsSuccess)
            return Result<UpdatePayrollRecordCommandResponse>.GeneralFailure(employmentResult.ErrorMessage!);

        var hasOverlappingPeriod = await payrollRecordQuery.HasOverlappingPeriodAsync(
            request.UserId,
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            request.PayrollRecordId,
            cancellationToken);
        if (hasOverlappingPeriod)
            return Result<UpdatePayrollRecordCommandResponse>.GeneralFailure("برای این کارمند در این بازه فیش پرداختی دیگری ثبت شده است.");

        var workshopTask = workShopRepository.GetByIdAsync(request.UserId, employee.WorkshopId, cancellationToken);
        var salaryProfilesTask = salaryDecreeQuery.GetSalaryDecreesAffectingPeriodAsync(
            request.UserId,
            request.EmployeeId,
            period.StartPeriod,
            period.EndPeriod,
            cancellationToken);

        await Task.WhenAll(workshopTask, salaryProfilesTask);

        var workshop = await workshopTask;
        var salaryProfiles = await salaryProfilesTask;

        if (workshop is null)
            return Result<UpdatePayrollRecordCommandResponse>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");

        if (salaryProfiles.Count == 0)
            return Result<UpdatePayrollRecordCommandResponse>.NotfoundFailure("برای این بازه حکم حقوقی کارمند یافت نشد.");

        var workInput = new PayrollWorkInput(
            request.Work.WorkedDaysCount,
            request.Work.OvertimeHours,
            request.Work.NightShiftHours,
            request.Work.FridayWorkHours,
            request.Work.LeaveHours,
            request.Work.AbsenceDaysCount,
            request.Work.MissionDaysCount,
            request.Work.MissionHours,
            request.Work.HolidayWorkHours,
            request.Work.MissionAmountOverride,
            standardWorkingDaysCount,
            isEsfandPeriod,
            request.Work.PerformanceBonusAmount,
            request.Work.CashBenefitsAmount,
            request.Work.AnnualBonusType
        );

        var calculationResult = await payrollCalculationService.CalculateAsync(
            employee,
            workshop,
            salaryProfiles,
            period.StartPeriod,
            period.EndPeriod,
            workInput,
            cancellationToken);
        if (!calculationResult.IsSuccess)
            return Result<UpdatePayrollRecordCommandResponse>.ValidationFailure(calculationResult.Errors!);

        var calculation = calculationResult.Response!;
        var updateResult = payrollRecord.Update(
            period.StartPeriod,
            period.EndPeriod,
            salaryProfiles[0].IsTaxSubject,
            limits.MaxMonthlyOvertimeHours,
            limits.MaxFridayHours,
            workInput,
            calculation.Amounts,
            calculation.CalculatedAmounts);
        if (!updateResult.IsSuccess)
            return Result<UpdatePayrollRecordCommandResponse>.GeneralFailure(updateResult.ErrorMessage!);

        var isUpdated = await payrollRecordRepository.UpdateAsync(payrollRecord, cancellationToken);
        if (!isUpdated)
            return Result<UpdatePayrollRecordCommandResponse>.GeneralFailure("خطا در بروزرسانی فیش پرداختی");

        return Result<UpdatePayrollRecordCommandResponse>.Success(
            new UpdatePayrollRecordCommandResponse(
                payrollRecord.Id,
                calculation.CalculatedAmounts,
                calculation.Amounts));
    }
}