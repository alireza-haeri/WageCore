namespace Application.Features.PayrollRecords;

public class GetPayrollRecordCalculationDetailsQueryHandler(
    IPayrollRecordRepository payrollRecordRepository,
    IEmployeeRepository employeeRepository,
    IWorkShopRepository workShopRepository,
    IPersianCalendarService persianCalendarService,
    IPayrollLimitsResolver payrollLimitsResolver,
    ISalaryDecreeQuery salaryDecreeQuery,
    ILaborLawRuleQuery laborLawRuleQuery,
    IPayrollRecordQuery payrollRecordQuery,
    IPayrollCalculationService payrollCalculationService)
    : IRequestHandler<GetPayrollRecordCalculationDetailsQuery, Result<GetPayrollRecordCalculationDetailsQueryResponse>>
{
    // The same deprecated keys the calculation service itself ignores.
    private static readonly LaborLawRuleKey[] IgnoredRuleKeys =
    [
        LaborLawRuleKey.TaxExemptMonthlyAmount,
        LaborLawRuleKey.TaxRatePercentage
    ];

    public async Task<Result<GetPayrollRecordCalculationDetailsQueryResponse>> Handle(
        GetPayrollRecordCalculationDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var payrollRecord = await payrollRecordRepository.GetByIdAsync(
            request.UserId,
            request.PayrollRecordId,
            cancellationToken);

        if (payrollRecord is null)
            return Result<GetPayrollRecordCalculationDetailsQueryResponse>.NotfoundFailure(
                "فیش پرداختی مورد نظر یافت نشد.");

        var employee = await employeeRepository.GetByIdAsync(
            request.UserId,
            payrollRecord.EmployeeId,
            cancellationToken);

        if (employee is null)
            return Result<GetPayrollRecordCalculationDetailsQueryResponse>.NotfoundFailure(
                "کارمند مورد نظر یافت نشد.");

        var workshop = await workShopRepository.GetByIdAsync(
            request.UserId,
            employee.WorkshopId,
            cancellationToken);

        if (workshop is null)
            return Result<GetPayrollRecordCalculationDetailsQueryResponse>.NotfoundFailure(
                "کارگاه مورد نظر یافت نشد.");

        var limitsResult = await payrollLimitsResolver.ResolveAsync(
            payrollRecord.PeriodStart,
            payrollRecord.PeriodEnd,
            cancellationToken);

        if (!limitsResult.IsSuccess)
            return Result<GetPayrollRecordCalculationDetailsQueryResponse>.ValidationFailure(limitsResult.Errors!);

        var limits = limitsResult.Response!;

        var salaryDecrees = await salaryDecreeQuery.GetSalaryDecreesAffectingPeriodAsync(
            request.UserId,
            employee.Id,
            payrollRecord.PeriodStart,
            payrollRecord.PeriodEnd,
            cancellationToken);

        // The same decree the calculation uses: the latest one effective by the
        // end of the period.
        var salaryDecree = salaryDecrees
            .Where(decree => decree.EffectiveFrom <= payrollRecord.PeriodEnd)
            .OrderByDescending(decree => decree.EffectiveFrom)
            .FirstOrDefault();

        if (salaryDecree is null)
            return Result<GetPayrollRecordCalculationDetailsQueryResponse>.NotfoundFailure(
                "حکم حقوقی فعال برای این کارمند در این بازه یافت نشد.");

        var previousAnnualWorkedDaysCount = await payrollRecordQuery.GetAnnualWorkedDaysCountAsync(
            request.UserId,
            employee.Id,
            payrollRecord.PeriodStart,
            cancellationToken);

        var ruleValues = new List<PayrollCalculationRuleValue>();
        foreach (var ruleKey in Enum.GetValues<LaborLawRuleKey>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IgnoredRuleKeys.Contains(ruleKey))
                continue;

            var ruleValue = await laborLawRuleQuery.GetActiveValueAsync(
                ruleKey,
                payrollRecord.PeriodStart,
                cancellationToken);

            if (ruleValue is null)
                continue;

            ruleValues.Add(new PayrollCalculationRuleValue(ruleKey, ruleValue.Value));
        }

        var workInput = new PayrollWorkInput(
            payrollRecord.WorkedDaysCount,
            payrollRecord.OvertimeHours,
            payrollRecord.NightShiftHours,
            payrollRecord.FridayWorkHours,
            payrollRecord.LeaveHours,
            payrollRecord.AbsenceDaysCount,
            payrollRecord.MissionDaysCount,
            payrollRecord.MissionHours,
            payrollRecord.HolidayWorkHours,
            payrollRecord.MissionAmountOverride,
            payrollRecord.StandardWorkingDaysCount,
            payrollRecord.IsEsfandPeriod,
            payrollRecord.PerformanceBonusAmount,
            payrollRecord.CashBenefitsAmount,
            payrollRecord.AnnualBonusType);

        var calculationResult = await payrollCalculationService.CalculateAsync(
            employee,
            workshop,
            salaryDecrees,
            payrollRecord.PeriodStart,
            payrollRecord.PeriodEnd,
            workInput,
            cancellationToken);

        if (!calculationResult.IsSuccess)
            return Result<GetPayrollRecordCalculationDetailsQueryResponse>.ValidationFailure(calculationResult.Errors!);

        var calculation = calculationResult.Response!;

        return Result<GetPayrollRecordCalculationDetailsQueryResponse>.Success(
            new GetPayrollRecordCalculationDetailsQueryResponse(
                PayrollRecordId: payrollRecord.Id,
                EmployeeId: payrollRecord.EmployeeId,
                EmployeeName: employee.FullName,
                PersonalCode: employee.PersonalCode,
                EmployeeHireDate: employee.HireDate,
                Status: payrollRecord.Status,
                PersianYear: persianCalendarService.GetPersianYear(payrollRecord.PeriodStart),
                PersianMonth: persianCalendarService.GetPersianMonth(payrollRecord.PeriodStart),
                PeriodStart: payrollRecord.PeriodStart,
                PeriodEnd: payrollRecord.PeriodEnd,
                PeriodDaysCount: payrollRecord.PeriodEnd.DayNumber - payrollRecord.PeriodStart.DayNumber + 1,
                FridayCount: persianCalendarService.GetFridayCount(payrollRecord.PeriodStart, payrollRecord.PeriodEnd),
                DaysInYear: persianCalendarService.GetDaysInPersianYear(payrollRecord.PeriodStart),
                StandardWorkingDaysCount: payrollRecord.StandardWorkingDaysCount,
                WorkedDaysCount: payrollRecord.WorkedDaysCount,
                LeaveHours: payrollRecord.LeaveHours,
                AbsenceDaysCount: payrollRecord.AbsenceDaysCount,
                OvertimeHours: payrollRecord.OvertimeHours,
                NightShiftHours: payrollRecord.NightShiftHours,
                FridayWorkHours: payrollRecord.FridayWorkHours,
                HolidayWorkHours: payrollRecord.HolidayWorkHours,
                MissionDaysCount: payrollRecord.MissionDaysCount,
                MissionHours: payrollRecord.MissionHours,
                MissionAmountOverride: payrollRecord.MissionAmountOverride,
                PerformanceBonusAmount: payrollRecord.PerformanceBonusAmount,
                CashBenefitsAmount: payrollRecord.CashBenefitsAmount,
                AnnualBonusType: payrollRecord.AnnualBonusType,
                IsEsfandPeriod: payrollRecord.IsEsfandPeriod,
                PreviousAnnualWorkedDaysCount: previousAnnualWorkedDaysCount,
                AnnualWorkedDaysCount: previousAnnualWorkedDaysCount + payrollRecord.WorkedDaysCount,
                MaxMonthlyOvertimeHours: limits.MaxMonthlyOvertimeHours,
                MaxFridayHours: limits.MaxFridayHours,
                MaxNightShiftHours: limits.MaxNightShiftHours,
                DailyWorkingHours: limits.DailyWorkingHours,
                DecreeEffectiveFrom: salaryDecree.EffectiveFrom,
                BaseDailySalary: salaryDecree.BaseDailySalary,
                AttractionAllowance: salaryDecree.AttractionAllowance,
                SupervisionAllowance: salaryDecree.SupervisionAllowance,
                TransportationAllowanceNet: salaryDecree.TransportationAllowanceNet,
                ChildrenCount: salaryDecree.ChildrenCount,
                MaritalStatus: salaryDecree.MaritalStatus,
                ShiftType: salaryDecree.ShiftType,
                ContractType: salaryDecree.ContractType,
                IsTaxSubject: salaryDecree.IsTaxSubject,
                RuleValues: ruleValues,
                CalculatedAmounts: calculation.CalculatedAmounts,
                Amounts: calculation.Amounts));
    }
}
