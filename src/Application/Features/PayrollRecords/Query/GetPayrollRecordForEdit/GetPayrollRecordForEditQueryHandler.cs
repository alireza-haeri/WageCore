namespace Application.Features.PayrollRecords;

public class GetPayrollRecordForEditQueryHandler(
    IPayrollRecordRepository payrollRecordRepository,
    IEmployeeRepository employeeRepository,
    IPersianCalendarService persianCalendarService,
    IPayrollLimitsResolver payrollLimitsResolver)
    : IRequestHandler<GetPayrollRecordForEditQuery, Result<GetPayrollRecordForEditQueryResponse>>
{
    public async Task<Result<GetPayrollRecordForEditQueryResponse>> Handle(
        GetPayrollRecordForEditQuery request,
        CancellationToken cancellationToken)
    {
        var payrollRecord = await payrollRecordRepository.GetByIdAsync(
            request.UserId,
            request.PayrollRecordId,
            cancellationToken);

        if (payrollRecord is null)
            return Result<GetPayrollRecordForEditQueryResponse>.NotfoundFailure(
                "فیش پرداختی مورد نظر یافت نشد.");

        var employee = await employeeRepository.GetByIdAsync(
            request.UserId,
            payrollRecord.EmployeeId,
            cancellationToken);

        if (employee is null)
            return Result<GetPayrollRecordForEditQueryResponse>.NotfoundFailure(
                "کارمند مورد نظر یافت نشد.");

        var limitsResult = await payrollLimitsResolver.ResolveAsync(
            payrollRecord.PeriodStart,
            payrollRecord.PeriodEnd,
            cancellationToken);

        if (!limitsResult.IsSuccess)
            return Result<GetPayrollRecordForEditQueryResponse>.ValidationFailure(limitsResult.Errors!);

        var limits = limitsResult.Response!;

        var work = new UserWorkInputDto(
            payrollRecord.WorkedDaysCount,
            PayrollWorkInputMapper.FromHours(payrollRecord.OvertimeHours),
            PayrollWorkInputMapper.FromHours(payrollRecord.NightShiftHours),
            PayrollWorkInputMapper.FromHours(payrollRecord.FridayWorkHours),
            PayrollWorkInputMapper.FromHours(payrollRecord.HolidayWorkHours),
            PayrollWorkInputMapper.FromHours(payrollRecord.LeaveHours, limits.DailyWorkingHours),
            (int)payrollRecord.MissionDaysCount,
            PayrollWorkInputMapper.FromHours(payrollRecord.MissionHours),
            payrollRecord.HolidaysCount,
            payrollRecord.MissionAmountOverride,
            payrollRecord.PerformanceBonusAmount,
            payrollRecord.CashBenefitsAmount,
            payrollRecord.AnnualBonusType);

        return Result<GetPayrollRecordForEditQueryResponse>.Success(
            new GetPayrollRecordForEditQueryResponse(
                payrollRecord.Id,
                payrollRecord.EmployeeId,
                employee.FullName,
                employee.PersonalCode,
                persianCalendarService.GetPersianYear(payrollRecord.PeriodStart),
                persianCalendarService.GetPersianMonth(payrollRecord.PeriodStart),
                work,
                payrollRecord.Status));
    }
}
