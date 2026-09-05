using Core.Contracts.PayrollRecords;
using Core.Domain;
using Core.Domain.Enums;

namespace Shared.Tests.Builders;

public class PayrollRecordBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _employeeId = Guid.NewGuid();
    private decimal? _maxMonthlyOvertimeHours = 20m;
    private decimal? _maxFridayHours = 12m;
    private decimal? _maxNightShiftHours = 3m;
    private decimal? _dailyWorkingHours = 8m;
    private DateOnly _periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
    private DateOnly _periodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private int _workedDaysCount = 24;
    private decimal _overtimeHours = 4m;
    private decimal _nightShiftHours = 3m;
    private decimal _fridayWorkHours = 2m;
    private decimal _leaveHours = 2m;
    private int _holidaysCount = 0;
    private decimal _missionDaysCount = 1m;
    private decimal _missionHours = 0m;
    private decimal _holidayWorkHours = 0m;
    private decimal? _missionAmountOverride;
    private int _standardWorkingDaysCount = 31;
    private bool _isEsfandPeriod;
    private AnnualBonusType? _annualBonusType;
    private decimal? _performanceBonusAmount;
    private decimal? _cashBenefitsAmount;
    private decimal _overtimeAmount = 800_000m;
    private decimal _nightShiftExtraAmount = 300_000m;
    private decimal _fridayWorkAllowance = 250_000m;
    private decimal _calculatedTaxAmount = 1_500_000m;
    private decimal _netPayableAmount = 15_000_000m;
    private decimal _grossAmount = 17_900_000m;
    private decimal _insuranceAmount = 1_400_000m;
    private decimal _totalDeductionsAmount = 2_900_000m;
    private decimal _baseSalaryAmount = 10_000_000m;
    private decimal _attractionAllowanceAmount;
    private decimal _supervisionAllowanceAmount;
    private decimal _holidayWorkAmount;
    private decimal _childAllowanceAmount;
    private decimal _housingAllowanceAmount;
    private decimal _foodAllowanceAmount;
    private decimal _marriageAllowanceAmount;
    private decimal _shiftWorkAmount;
    private decimal _dailyMissionAmount;
    private decimal _endOfServiceAmount;
    private decimal _calculatedAnnualBonusAmount;
    private decimal _commutingAllowanceAmount;

    public PayrollRecordBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PayrollRecordBuilder WithEmployeeId(Guid employeeId)
    {
        _employeeId = employeeId;
        return this;
    }

    public PayrollRecordBuilder WithMaxMonthlyOvertimeHours(decimal? maxMonthlyOvertimeHours)
    {
        _maxMonthlyOvertimeHours = maxMonthlyOvertimeHours;
        return this;
    }

    public PayrollRecordBuilder WithMaxFridayHours(decimal? maxFridayHours)
    {
        _maxFridayHours = maxFridayHours;
        return this;
    }

    public PayrollRecordBuilder WithMaxNightShiftHours(decimal? maxNightShiftHours)
    {
        _maxNightShiftHours = maxNightShiftHours;
        return this;
    }

    public PayrollRecordBuilder WithDailyWorkingHours(decimal? dailyWorkingHours)
    {
        _dailyWorkingHours = dailyWorkingHours;
        return this;
    }

    public PayrollRecordBuilder WithPeriod(DateOnly periodStart, DateOnly periodEnd)
    {
        _periodStart = periodStart;
        _periodEnd = periodEnd;
        return this;
    }

    public PayrollRecordBuilder WithWorkedDaysCount(int workedDaysCount)
    {
        _workedDaysCount = workedDaysCount;
        return this;
    }

    public PayrollRecordBuilder WithOvertimeHours(decimal overtimeHours)
    {
        _overtimeHours = overtimeHours;
        return this;
    }

    public PayrollRecordBuilder WithNightShiftHours(decimal nightShiftHours)
    {
        _nightShiftHours = nightShiftHours;
        return this;
    }

    public PayrollRecordBuilder WithFridayWorkHours(decimal fridayWorkHours)
    {
        _fridayWorkHours = fridayWorkHours;
        return this;
    }

    public PayrollRecordBuilder WithLeaveHours(decimal leaveHours)
    {
        _leaveHours = leaveHours;
        return this;
    }

    public PayrollRecordBuilder WithHolidaysCount(int holidaysCount)
    {
        _holidaysCount = holidaysCount;
        return this;
    }

    public PayrollRecordBuilder WithMissionDaysCount(decimal missionDaysCount)
    {
        _missionDaysCount = missionDaysCount;
        return this;
    }

    public PayrollRecordBuilder WithMissionHours(decimal missionHours)
    {
        _missionHours = missionHours;
        return this;
    }

    public PayrollRecordBuilder WithHolidayWorkHours(decimal holidayWorkHours)
    {
        _holidayWorkHours = holidayWorkHours;
        return this;
    }

    public PayrollRecordBuilder WithMissionAmountOverride(decimal? missionAmountOverride)
    {
        _missionAmountOverride = missionAmountOverride;
        return this;
    }

    public PayrollRecordBuilder WithStandardWorkingDaysCount(int standardWorkingDaysCount)
    {
        _standardWorkingDaysCount = standardWorkingDaysCount;
        return this;
    }

    public PayrollRecordBuilder WithIsEsfandPeriod(bool isEsfandPeriod)
    {
        _isEsfandPeriod = isEsfandPeriod;
        return this;
    }

    public PayrollRecordBuilder WithAnnualBonusType(AnnualBonusType? annualBonusType)
    {
        _annualBonusType = annualBonusType;
        return this;
    }

    public PayrollRecordBuilder WithPerformanceBonusAmount(decimal? performanceBonusAmount)
    {
        _performanceBonusAmount = performanceBonusAmount;
        return this;
    }

    public PayrollRecordBuilder WithCashBenefitsAmount(decimal? cashBenefitsAmount)
    {
        _cashBenefitsAmount = cashBenefitsAmount;
        return this;
    }

    public PayrollRecordBuilder WithOvertimeAmount(decimal overtimeAmount)
    {
        _overtimeAmount = overtimeAmount;
        return this;
    }

    public PayrollRecordBuilder WithNightShiftExtraAmount(decimal nightShiftExtraAmount)
    {
        _nightShiftExtraAmount = nightShiftExtraAmount;
        return this;
    }

    public PayrollRecordBuilder WithFridayWorkAllowance(decimal fridayWorkAllowance)
    {
        _fridayWorkAllowance = fridayWorkAllowance;
        return this;
    }

    public PayrollRecordBuilder WithCalculatedTaxAmount(decimal calculatedTaxAmount)
    {
        _calculatedTaxAmount = calculatedTaxAmount;
        return this;
    }

    public PayrollRecordBuilder WithNetPayableAmount(decimal netPayableAmount)
    {
        _netPayableAmount = netPayableAmount;
        return this;
    }

    public PayrollRecordBuilder WithGrossAmount(decimal grossAmount)
    {
        _grossAmount = grossAmount;
        return this;
    }

    public PayrollRecordBuilder WithInsuranceAmount(decimal insuranceAmount)
    {
        _insuranceAmount = insuranceAmount;
        return this;
    }

    public PayrollRecordBuilder WithTotalDeductionsAmount(decimal totalDeductionsAmount)
    {
        _totalDeductionsAmount = totalDeductionsAmount;
        return this;
    }

    public PayrollRecordBuilder WithBaseSalaryAmount(decimal baseSalaryAmount)
    {
        _baseSalaryAmount = baseSalaryAmount;
        return this;
    }

    public PayrollRecordBuilder WithAttractionAllowanceAmount(decimal attractionAllowanceAmount)
    {
        _attractionAllowanceAmount = attractionAllowanceAmount;
        return this;
    }

    public PayrollRecordBuilder WithSupervisionAllowanceAmount(decimal supervisionAllowanceAmount)
    {
        _supervisionAllowanceAmount = supervisionAllowanceAmount;
        return this;
    }

    public PayrollRecordBuilder WithHolidayWorkAmount(decimal holidayWorkAmount)
    {
        _holidayWorkAmount = holidayWorkAmount;
        return this;
    }

    public PayrollRecordBuilder WithChildAllowanceAmount(decimal childAllowanceAmount)
    {
        _childAllowanceAmount = childAllowanceAmount;
        return this;
    }

    public PayrollRecordBuilder WithHousingAllowanceAmount(decimal housingAllowanceAmount)
    {
        _housingAllowanceAmount = housingAllowanceAmount;
        return this;
    }

    public PayrollRecordBuilder WithFoodAllowanceAmount(decimal foodAllowanceAmount)
    {
        _foodAllowanceAmount = foodAllowanceAmount;
        return this;
    }

    public PayrollRecordBuilder WithMarriageAllowanceAmount(decimal marriageAllowanceAmount)
    {
        _marriageAllowanceAmount = marriageAllowanceAmount;
        return this;
    }

    public PayrollRecordBuilder WithShiftWorkAmount(decimal shiftWorkAmount)
    {
        _shiftWorkAmount = shiftWorkAmount;
        return this;
    }

    public PayrollRecordBuilder WithDailyMissionAmount(decimal dailyMissionAmount)
    {
        _dailyMissionAmount = dailyMissionAmount;
        return this;
    }

    public PayrollRecordBuilder WithEndOfServiceAmount(decimal endOfServiceAmount)
    {
        _endOfServiceAmount = endOfServiceAmount;
        return this;
    }

    public PayrollRecordBuilder WithCalculatedAnnualBonusAmount(decimal calculatedAnnualBonusAmount)
    {
        _calculatedAnnualBonusAmount = calculatedAnnualBonusAmount;
        return this;
    }

    public PayrollRecordBuilder WithCommutingAllowanceAmount(decimal commutingAllowanceAmount)
    {
        _commutingAllowanceAmount = commutingAllowanceAmount;
        return this;
    }

    public UserWorkInputDto BuildUserWorkInputDto() =>
        new(
            _workedDaysCount,
            ToWorkTimeInput(_overtimeHours),
            ToWorkTimeInput(_nightShiftHours),
            ToWorkTimeInput(_fridayWorkHours),
            ToWorkTimeInput(_holidayWorkHours),
            ToDayTimeInput(_leaveHours),
            (int)_missionDaysCount,
            ToWorkTimeInput(_missionHours),
            _holidaysCount,
            _missionAmountOverride,
            _performanceBonusAmount,
            _cashBenefitsAmount,
            _annualBonusType);

    private static WorkTimeInput ToWorkTimeInput(decimal hours)
    {
        var wholeHours = (int)hours;
        var minutes = (int)Math.Round((hours - wholeHours) * 60m, MidpointRounding.AwayFromZero);
        return new WorkTimeInput(wholeHours, minutes);
    }

    private static DayTimeInput ToDayTimeInput(decimal hours)
    {
        var workTime = ToWorkTimeInput(hours);
        return new DayTimeInput(0, workTime.Hours, workTime.Minutes);
    }

    public PayrollWorkInput BuildPayrollWorkInput() =>
        new(
            _workedDaysCount,
            _overtimeHours,
            _nightShiftHours,
            _fridayWorkHours,
            _leaveHours,
            _missionDaysCount,
            _missionHours,
            _holidayWorkHours,
            _holidaysCount,
            _missionAmountOverride,
            _standardWorkingDaysCount,
            _isEsfandPeriod,
            _performanceBonusAmount,
            _cashBenefitsAmount,
            _annualBonusType);

    public PayrollRecordAmountsDto BuildAmountsDto() =>
        new(
            _calculatedTaxAmount,
            _grossAmount,
            _insuranceAmount,
            _totalDeductionsAmount,
            _netPayableAmount);

    public PayrollCalculatedAmountsDto BuildCalculatedAmountsDto() =>
        new(
            _baseSalaryAmount,
            _attractionAllowanceAmount,
            _supervisionAllowanceAmount,
            _nightShiftExtraAmount,
            _holidayWorkAmount,
            _childAllowanceAmount,
            _housingAllowanceAmount,
            _foodAllowanceAmount,
            _marriageAllowanceAmount,
            _overtimeAmount,
            _shiftWorkAmount,
            _dailyMissionAmount,
            _fridayWorkAllowance,
            _endOfServiceAmount,
            _calculatedAnnualBonusAmount,
            _commutingAllowanceAmount,
            _performanceBonusAmount,
            _cashBenefitsAmount);

    public DomainResult<PayrollRecord> CreateResult()
    {
        return PayrollRecord.Create(
            _id,
            _employeeId,
            _periodStart,
            _periodEnd,
            _maxMonthlyOvertimeHours,
            _maxFridayHours,
            _maxNightShiftHours,
            _dailyWorkingHours,
            BuildPayrollWorkInput(),
            BuildAmountsDto(),
            BuildCalculatedAmountsDto());
    }
}