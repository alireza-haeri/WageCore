namespace Application.Features.PayrollRecords;

public class GetPayrollRecordCalculationDetailsQueryHandler(
    IPayrollRecordRepository payrollRecordRepository,
    IEmployeeRepository employeeRepository,
    IPersianCalendarService persianCalendarService,
    ISalaryDecreeQuery salaryDecreeQuery)
    : IRequestHandler<GetPayrollRecordCalculationDetailsQuery, Result<GetPayrollRecordCalculationDetailsQueryResponse>>
{
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

        var salaryDecrees = await salaryDecreeQuery.GetSalaryDecreesAffectingPeriodAsync(
            request.UserId,
            employee.Id,
            payrollRecord.PeriodStart,
            payrollRecord.PeriodEnd,
            cancellationToken);

        // The decree in effect at the end of the period. Decrees affecting a
        // paid period are locked (create/update/delete blocked), so these
        // values are stable for the life of this payslip.
        var salaryDecree = salaryDecrees
            .Where(decree => decree.EffectiveFrom <= payrollRecord.PeriodEnd)
            .OrderByDescending(decree => decree.EffectiveFrom)
            .FirstOrDefault();

        if (salaryDecree is null)
            return Result<GetPayrollRecordCalculationDetailsQueryResponse>.NotfoundFailure(
                "حکم حقوقی فعال برای این کارمند در این بازه یافت نشد.");

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
                MaxMonthlyOvertimeHours: payrollRecord.MaxMonthlyOvertimeHours,
                MaxFridayHours: payrollRecord.MaxFridayHours,
                MaxNightShiftHours: payrollRecord.MaxNightShiftHours,
                DailyWorkingHours: payrollRecord.DailyWorkingHours,
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
                CalculatedAmounts: new PayrollCalculatedAmountsDto(
                    payrollRecord.BaseSalaryAmount,
                    payrollRecord.AttractionAllowanceAmount,
                    payrollRecord.SupervisionAllowanceAmount,
                    payrollRecord.NightShiftExtraAmount,
                    payrollRecord.HolidayWorkAmount,
                    payrollRecord.ChildAllowanceAmount,
                    payrollRecord.HousingAllowanceAmount,
                    payrollRecord.FoodAllowanceAmount,
                    payrollRecord.MarriageAllowanceAmount,
                    payrollRecord.OvertimeAmount,
                    payrollRecord.ShiftWorkAmount,
                    payrollRecord.DailyMissionAmount,
                    payrollRecord.FridayWorkAllowance,
                    payrollRecord.EndOfServiceAmount,
                    payrollRecord.AnnualBonusAmount,
                    payrollRecord.CommutingAllowanceAmount,
                    payrollRecord.PerformanceBonusAmount,
                    payrollRecord.CashBenefitsAmount),
                Amounts: new PayrollRecordAmountsDto(
                    payrollRecord.CalculatedTaxAmount,
                    payrollRecord.GrossAmount,
                    payrollRecord.InsuranceAmount,
                    payrollRecord.TotalDeductionsAmount,
                    payrollRecord.NetPayableAmount)));
    }
}
