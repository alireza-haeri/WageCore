namespace Shared.Tests.Builders;

public class EmployeeSalaryProfileBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _employeeId = Guid.NewGuid();
    private DateOnly? _employeeHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private DateOnly? _latestExistingEffectiveFrom;
    private decimal? _minimumMonthlySalary = 10_000_000m;
    private DateOnly? _effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
    private decimal? _baseMonthlySalary = 20_000_000m;
    private decimal? _attractionAllowance;
    private decimal? _supervisionAllowance;
    private SeniorityBaseApplicationMode? _seniorityBaseApplicationMode = SeniorityBaseApplicationMode.Manual;
    private SeniorityBaseCalculationMethod? _seniorityBaseCalculationMethod;
    private YearEndSeniorityMode? _yearEndSeniorityMode = YearEndSeniorityMode.MonthlyAccrual;
    private ShiftType? _shiftType = ShiftType.None;
    private decimal? _housingAllowance;
    private decimal? _foodAllowance;
    private decimal? _childAllowancePerChild;
    private decimal? _transportationAllowanceNet;
    private decimal? _karanehAmountNet;

    public EmployeeSalaryProfileBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithEmployeeId(Guid employeeId)
    {
        _employeeId = employeeId;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithEmployeeHireDate(DateOnly? employeeHireDate)
    {
        _employeeHireDate = employeeHireDate;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithLatestExistingEffectiveFrom(DateOnly? latestExistingEffectiveFrom)
    {
        _latestExistingEffectiveFrom = latestExistingEffectiveFrom;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithMinimumMonthlySalary(decimal? minimumMonthlySalary)
    {
        _minimumMonthlySalary = minimumMonthlySalary;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithEffectiveFrom(DateOnly? effectiveFrom)
    {
        _effectiveFrom = effectiveFrom;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithBaseMonthlySalary(decimal? baseMonthlySalary)
    {
        _baseMonthlySalary = baseMonthlySalary;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithAttractionAllowance(decimal? attractionAllowance)
    {
        _attractionAllowance = attractionAllowance;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithSupervisionAllowance(decimal? supervisionAllowance)
    {
        _supervisionAllowance = supervisionAllowance;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithSeniorityBaseApplicationMode(
        SeniorityBaseApplicationMode? seniorityBaseApplicationMode)
    {
        _seniorityBaseApplicationMode = seniorityBaseApplicationMode;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithSeniorityBaseCalculationMethod(
        SeniorityBaseCalculationMethod? seniorityBaseCalculationMethod)
    {
        _seniorityBaseCalculationMethod = seniorityBaseCalculationMethod;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithYearEndSeniorityMode(YearEndSeniorityMode? yearEndSeniorityMode)
    {
        _yearEndSeniorityMode = yearEndSeniorityMode;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithShiftType(ShiftType? shiftType)
    {
        _shiftType = shiftType;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithHousingAllowance(decimal? housingAllowance)
    {
        _housingAllowance = housingAllowance;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithFoodAllowance(decimal? foodAllowance)
    {
        _foodAllowance = foodAllowance;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithChildAllowancePerChild(decimal? childAllowancePerChild)
    {
        _childAllowancePerChild = childAllowancePerChild;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithTransportationAllowanceNet(decimal? transportationAllowanceNet)
    {
        _transportationAllowanceNet = transportationAllowanceNet;
        return this;
    }

    public EmployeeSalaryProfileBuilder WithKaranehAmountNet(decimal? karanehAmountNet)
    {
        _karanehAmountNet = karanehAmountNet;
        return this;
    }

    public EmployeeSalaryProfileDto BuildDto() =>
        new(
            _effectiveFrom,
            _baseMonthlySalary,
            _attractionAllowance,
            _supervisionAllowance,
            _seniorityBaseApplicationMode,
            _seniorityBaseCalculationMethod,
            _yearEndSeniorityMode,
            _shiftType,
            _housingAllowance,
            _foodAllowance,
            _childAllowancePerChild,
            _transportationAllowanceNet,
            _karanehAmountNet);

    public DomainResult<EmployeeSalaryProfile> CreateResult()
    {
        return EmployeeSalaryProfile.Create(
            _id,
            _employeeId,
            _employeeHireDate,
            _latestExistingEffectiveFrom,
            _minimumMonthlySalary,
            BuildDto());
    }
}
