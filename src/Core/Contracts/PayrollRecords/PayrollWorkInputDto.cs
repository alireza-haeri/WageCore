using Core.Domain.Enums;

namespace Core.Contracts.PayrollRecords;

public record UserWorkInputDto(
    int WorkedDaysCount,
    WorkTimeInput Overtime,
    WorkTimeInput NightShift,
    WorkTimeInput FridayWork,
    WorkTimeInput HolidayWork,
    DayTimeInput Leave,
    int MissionDays,
    WorkTimeInput MissionHours,
    int HolidaysCount,
    decimal? MissionAmountOverride,
    decimal? PerformanceBonusAmount,
    decimal? CashBenefitsAmount,
    AnnualBonusType? AnnualBonusType
);
