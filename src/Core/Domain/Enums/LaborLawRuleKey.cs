namespace Core.Domain.Enums;

public enum LaborLawRuleKey
{
    MinimumDailySalary = 0,
    DailyWorkingHours = 1,
    MaximumOvertimeHoursPerDay = 2,
    MaximumNightShiftHoursPerDay = 3,
    MaximumOvertimeHoursPerMonth = 4,
    MaximumFridayWorkHoursPerMonth = 5,
    InsurancePercentage = 6,
    AnnualBonusMinimumAmount = 7,
    AnnualBonusMaximumAmount = 8,

    // Deprecated: kept for historical rows only; tax is fully bracket-driven now
    // (see the TaxBracket* keys below), so these two are no longer read anywhere.
    TaxExemptMonthlyAmount = 9,
    TaxRatePercentage = 10,

    StandardDailyWorkHours = 11,
    NightShiftPercentage = 12,
    HolidayWorkPercentage = 13,
    OvertimePercentage = 14,
    FridayWorkPercentage = 15,
    ChildAllowanceMultiplier = 16,
    EndOfServiceDaysPerYear = 17,
    HousingAllowanceAmount = 18,
    FoodAllowanceAmount = 19,
    MarriageAllowanceAmount = 20,

    ShiftWorkPercentageMorningEvening = 21,
    ShiftWorkPercentageMorningNight = 22,
    ShiftWorkPercentageEveningNight = 23,
    ShiftWorkPercentageMorningEveningNight = 24,

}
