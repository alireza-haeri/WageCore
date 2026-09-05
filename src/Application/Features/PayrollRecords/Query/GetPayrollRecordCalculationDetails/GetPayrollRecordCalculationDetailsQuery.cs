namespace Application.Features.PayrollRecords;

public record GetPayrollRecordCalculationDetailsQuery(Guid UserId, Guid PayrollRecordId)
    : IRequest<Result<GetPayrollRecordCalculationDetailsQueryResponse>>;

/// <summary>
/// The persisted payroll record as it was calculated and saved: work inputs,
/// the limits in effect at save time, the (locked) salary decree values, and
/// the itemized calculated amounts. No calculation is performed here.
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
    int WorkedDaysCount,
    int HolidaysCount,
    decimal LeaveHours,
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
    PayrollCalculatedAmountsDto CalculatedAmounts,
    PayrollRecordAmountsDto Amounts);
