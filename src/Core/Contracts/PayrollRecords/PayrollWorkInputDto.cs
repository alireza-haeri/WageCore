using Core.Domain.Enums;

namespace Core.Contracts.PayrollRecords;

public record PayrollWorkInputDto(
    decimal? WorkedDaysCount,
    decimal? OvertimeHours,
    decimal? NightShiftHours,
    decimal? FridayWorkHours,
    decimal? LeaveHours,
    decimal? AbsenceDaysCount,
    decimal? MissionDaysCount,
    decimal? MissionHours,
    decimal? HolidayWorkHours,
    decimal? MissionAmountOverride,
    int? StandardWorkingDaysCount,
    bool IsEsfandPeriod,
    decimal? AnnualBonusAmount,
    AnnualBonusType? AnnualBonusType,
    decimal? PerformanceBonusAmount,
    decimal? CashBenefitsAmount);
