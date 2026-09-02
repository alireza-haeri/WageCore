namespace Shared.Tests.Builders;

public class PayrollRecordBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _employeeId = Guid.NewGuid();
    private bool _employeeIsTaxSubject;
    private decimal? _maxMonthlyOvertimeHours = 20m;
    private decimal? _maxFridayHours = 12m;
    private DateOnly _periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
    private DateOnly _periodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private decimal? _workedDaysCount = 24m;
    private decimal? _overtimeHours = 4m;
    private decimal? _nightShiftHours = 3m;
    private decimal? _fridayWorkHours = 2m;
    private decimal? _leaveHours = 2m;
    private decimal? _absenceDaysCount = 0m;
    private decimal? _missionDaysCount = 1m;
    private decimal? _missionHours = 0m;
    private decimal? _holidayWorkHours = 0m;
    private decimal? _missionAmountOverride;
    private int? _standardWorkingDaysCount = 31;
    private bool _isEsfandPeriod;
    private decimal? _annualBonusAmount;
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

    public PayrollRecordBuilder WithEmployeeIsTaxSubject(bool employeeIsTaxSubject)
    {
        _employeeIsTaxSubject = employeeIsTaxSubject;
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

    public PayrollRecordBuilder WithPeriod(DateOnly periodStart, DateOnly periodEnd)
    {
        _periodStart = periodStart;
        _periodEnd = periodEnd;
        return this;
    }

    public PayrollRecordBuilder WithWorkedDaysCount(decimal? workedDaysCount)
    {
        _workedDaysCount = workedDaysCount;
        return this;
    }

    public PayrollRecordBuilder WithOvertimeHours(decimal? overtimeHours)
    {
        _overtimeHours = overtimeHours;
        return this;
    }

    public PayrollRecordBuilder WithNightShiftHours(decimal? nightShiftHours)
    {
        _nightShiftHours = nightShiftHours;
        return this;
    }

    public PayrollRecordBuilder WithFridayWorkHours(decimal? fridayWorkHours)
    {
        _fridayWorkHours = fridayWorkHours;
        return this;
    }

    public PayrollRecordBuilder WithLeaveHours(decimal? leaveHours)
    {
        _leaveHours = leaveHours;
        return this;
    }

    public PayrollRecordBuilder WithAbsenceDaysCount(decimal? absenceDaysCount)
    {
        _absenceDaysCount = absenceDaysCount;
        return this;
    }

    public PayrollRecordBuilder WithMissionDaysCount(decimal? missionDaysCount)
    {
        _missionDaysCount = missionDaysCount;
        return this;
    }

    public PayrollRecordBuilder WithMissionHours(decimal? missionHours)
    {
        _missionHours = missionHours;
        return this;
    }

    public PayrollRecordBuilder WithHolidayWorkHours(decimal? holidayWorkHours)
    {
        _holidayWorkHours = holidayWorkHours;
        return this;
    }

    public PayrollRecordBuilder WithMissionAmountOverride(decimal? missionAmountOverride)
    {
        _missionAmountOverride = missionAmountOverride;
        return this;
    }

    public PayrollRecordBuilder WithStandardWorkingDaysCount(int? standardWorkingDaysCount)
    {
        _standardWorkingDaysCount = standardWorkingDaysCount;
        return this;
    }

    public PayrollRecordBuilder WithIsEsfandPeriod(bool isEsfandPeriod)
    {
        _isEsfandPeriod = isEsfandPeriod;
        return this;
    }

    public PayrollRecordBuilder WithAnnualBonusAmount(decimal? annualBonusAmount)
    {
        _annualBonusAmount = annualBonusAmount;
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

    public PayrollWorkInputDto BuildDto() =>
        new(
            _workedDaysCount,
            _overtimeHours,
            _nightShiftHours,
            _fridayWorkHours,
            _leaveHours,
            _absenceDaysCount,
            _missionDaysCount,
            _missionHours,
            _holidayWorkHours,
            _missionAmountOverride,
            _standardWorkingDaysCount,
            _isEsfandPeriod,
            _annualBonusAmount,
            _annualBonusType,
            _performanceBonusAmount,
            _cashBenefitsAmount);

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
            _commutingAllowanceAmount);

    public DomainResult<PayrollRecord> CreateResult()
    {
        return PayrollRecord.Create(
            _id,
            _employeeId,
            _periodStart,
            _periodEnd,
            _employeeIsTaxSubject,
            _maxMonthlyOvertimeHours,
            _maxFridayHours,
            BuildDto(),
            BuildAmountsDto(),
            BuildCalculatedAmountsDto());
    }
}
