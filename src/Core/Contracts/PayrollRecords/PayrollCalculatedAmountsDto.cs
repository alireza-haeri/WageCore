namespace Core.Contracts.PayrollRecords;

public record PayrollCalculatedAmountsDto(
    decimal BaseSalaryAmount,
    decimal AttractionAllowanceAmount,
    decimal SupervisionAllowanceAmount,
    decimal NightShiftExtraAmount,
    decimal HolidayWorkAmount,
    decimal ChildAllowanceAmount,
    decimal HousingAllowanceAmount,
    decimal FoodAllowanceAmount,
    decimal MarriageAllowanceAmount,
    decimal OvertimeAmount,
    decimal ShiftWorkAmount,
    decimal DailyMissionAmount,
    decimal FridayWorkAllowance,
    decimal EndOfServiceAmount,
    decimal? AnnualBonusAmount,
    decimal CommutingAllowanceAmount,
    decimal? PerformanceBonusAmount,
    decimal? CashBenefitsAmount);
