using Core.Domain.Enums;

namespace Core.Contracts.PayrollRecords;

public record UserWorkInputDto(
    decimal WorkedDaysCount,
    WorkTimeInput Overtime,
    WorkTimeInput NightShift,
    WorkTimeInput FridayWork,
    WorkTimeInput HolidayWork,
    DayTimeInput Leave,
    decimal AbsenceDaysCount,
    int MissionDays,
    WorkTimeInput MissionHours,
    decimal? MissionAmountOverride,
    decimal? PerformanceBonusAmount,
    decimal? CashBenefitsAmount,
    AnnualBonusType? AnnualBonusType
);
