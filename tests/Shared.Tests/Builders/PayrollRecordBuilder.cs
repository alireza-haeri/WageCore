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
    private decimal? _leaveDaysCount = 2m;
    private decimal? _absenceDaysCount = 0m;
    private decimal? _missionDaysCount = 1m;
    private decimal _overtimeAmount = 800_000m;
    private decimal _nightShiftExtraAmount = 300_000m;
    private decimal _fridayWorkAllowance = 250_000m;
    private decimal _calculatedTaxAmount = 1_500_000m;
    private decimal _netPayableAmount = 15_000_000m;

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

    public PayrollRecordBuilder WithLeaveDaysCount(decimal? leaveDaysCount)
    {
        _leaveDaysCount = leaveDaysCount;
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

    public PayrollWorkInputDto BuildDto() =>
        new(
            _workedDaysCount,
            _overtimeHours,
            _nightShiftHours,
            _fridayWorkHours,
            _leaveDaysCount,
            _absenceDaysCount,
            _missionDaysCount);

    public PayrollRecordAmountsDto BuildAmountsDto() =>
        new(
            _overtimeAmount,
            _nightShiftExtraAmount,
            _fridayWorkAllowance,
            _calculatedTaxAmount,
            _netPayableAmount);

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
            BuildAmountsDto());
    }
}
