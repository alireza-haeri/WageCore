namespace Application.Features.PayrollRecords;

public record GetPayrollRecordCalculationDetailsQuery(Guid UserId, Guid PayrollRecordId)
    : IRequest<Result<GetPayrollRecordCalculationDetailsQueryResponse>>;

public record PayrollCalculationRuleValue(LaborLawRuleKey Key, decimal Value);

/// <summary>
/// Everything the payroll calculation consumes and produces for a persisted
/// payroll record: the employee and salary decree inputs, the period and
/// annual context, the labor-law limits and rule values in effect, and the
/// full itemized result of re-running the calculation.
/// </summary>
public record GetPayrollRecordCalculationDetailsQueryResponse(
    Guid PayrollRecordId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    DateOnly EmployeeHireDate,
    PayrollRecordStatus Status,
    int PersianYear,
    int PersianMonth,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int PeriodDaysCount,
    int FridayCount,
    int DaysInYear,
    int StandardWorkingDaysCount,
    decimal WorkedDaysCount,
    decimal LeaveHours,
    decimal AbsenceDaysCount,
    decimal OvertimeHours,
    decimal NightShiftHours,
    decimal FridayWorkHours,
    decimal HolidayWorkHours,
    decimal MissionDaysCount,
    decimal MissionHours,
    decimal? MissionAmountOverride,
    decimal? PerformanceBonusAmount,
    decimal? CashBenefitsAmount,
    AnnualBonusType? AnnualBonusType,
    bool IsEsfandPeriod,
    decimal PreviousAnnualWorkedDaysCount,
    decimal AnnualWorkedDaysCount,
    decimal MaxMonthlyOvertimeHours,
    decimal MaxFridayHours,
    decimal MaxNightShiftHours,
    decimal DailyWorkingHours,
    DateOnly DecreeEffectiveFrom,
    decimal BaseDailySalary,
    decimal? AttractionAllowance,
    decimal? SupervisionAllowance,
    decimal? TransportationAllowanceNet,
    int ChildrenCount,
    EmployeeMaritalStatus MaritalStatus,
    ShiftType ShiftType,
    ContractType ContractType,
    bool IsTaxSubject,
    IReadOnlyList<PayrollCalculationRuleValue> RuleValues,
    PayrollCalculatedAmountsDto CalculatedAmounts,
    PayrollRecordAmountsDto Amounts);
