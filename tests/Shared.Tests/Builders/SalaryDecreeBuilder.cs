namespace Shared.Tests.Builders;

public class SalaryDecreeBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _employeeId = Guid.NewGuid();
    private DateOnly? _employeeHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private DateOnly? _latestExistingEffectiveFrom;
    private decimal? _minimumMonthlySalary = 10_000_000m;
    private DateOnly? _effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
    private decimal? _baseDailySalary = 20_000_000m;
    private decimal? _attractionAllowance;
    private decimal? _supervisionAllowance;
    private ShiftType? _shiftType = ShiftType.None;
    private ContractType? _contractType = ContractType.Permanent;
    private decimal? _housingAllowance;
    private decimal? _foodAllowance;
    private decimal? _transportationAllowanceNet;
    private decimal? _karanehAmountNet;
    private EmployeeMaritalStatus? _maritalStatus = EmployeeMaritalStatus.Single;
    private int? _childrenCount = 0;
    private bool? _isTaxSubject = true;
    private string _insuranceNumber = "INS-001";
    private string? _socialSecurityContractRow;
    private string _positionInInsuranceList = "اپراتور";
    private bool _isSubjectTo7PercentInsurance = true;
    private bool _isSubjectTo20PercentInsurance = true;
    private bool _isSubjectTo3PercentInsurance = false;
    private bool _isSubjectTo4PercentInsurance = false;
    private InsuranceCalculationProfile? _insuranceCalculationProfile = InsuranceCalculationProfile.FullLegal;

    public SalaryDecreeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SalaryDecreeBuilder WithEmployeeId(Guid employeeId)
    {
        _employeeId = employeeId;
        return this;
    }

    public SalaryDecreeBuilder WithEmployeeHireDate(DateOnly? employeeHireDate)
    {
        _employeeHireDate = employeeHireDate;
        return this;
    }

    public SalaryDecreeBuilder WithLatestExistingEffectiveFrom(DateOnly? latestExistingEffectiveFrom)
    {
        _latestExistingEffectiveFrom = latestExistingEffectiveFrom;
        return this;
    }

    public SalaryDecreeBuilder WithMinimumMonthlySalary(decimal? minimumMonthlySalary)
    {
        _minimumMonthlySalary = minimumMonthlySalary;
        return this;
    }

    public SalaryDecreeBuilder WithEffectiveFrom(DateOnly? effectiveFrom)
    {
        _effectiveFrom = effectiveFrom;
        return this;
    }

    public SalaryDecreeBuilder WithBaseDailySalary(decimal? baseDailySalary)
    {
        _baseDailySalary = baseDailySalary;
        return this;
    }

    public SalaryDecreeBuilder WithAttractionAllowance(decimal? attractionAllowance)
    {
        _attractionAllowance = attractionAllowance;
        return this;
    }

    public SalaryDecreeBuilder WithSupervisionAllowance(decimal? supervisionAllowance)
    {
        _supervisionAllowance = supervisionAllowance;
        return this;
    }

    public SalaryDecreeBuilder WithShiftType(ShiftType? shiftType)
    {
        _shiftType = shiftType;
        return this;
    }

    public SalaryDecreeBuilder WithContractType(ContractType? contractType)
    {
        _contractType = contractType;
        return this;
    }

    public SalaryDecreeBuilder WithHousingAllowance(decimal? housingAllowance)
    {
        _housingAllowance = housingAllowance;
        return this;
    }

    public SalaryDecreeBuilder WithFoodAllowance(decimal? foodAllowance)
    {
        _foodAllowance = foodAllowance;
        return this;
    }

    public SalaryDecreeBuilder WithTransportationAllowanceNet(decimal? transportationAllowanceNet)
    {
        _transportationAllowanceNet = transportationAllowanceNet;
        return this;
    }

    public SalaryDecreeBuilder WithKaranehAmountNet(decimal? karanehAmountNet)
    {
        _karanehAmountNet = karanehAmountNet;
        return this;
    }

    public SalaryDecreeBuilder WithMaritalStatus(EmployeeMaritalStatus? maritalStatus)
    {
        _maritalStatus = maritalStatus;
        return this;
    }

    public SalaryDecreeBuilder WithChildrenCount(int? childrenCount)
    {
        _childrenCount = childrenCount;
        return this;
    }

    public SalaryDecreeBuilder WithIsTaxSubject(bool? isTaxSubject)
    {
        _isTaxSubject = isTaxSubject;
        return this;
    }

    public SalaryDecreeBuilder WithInsuranceNumber(string insuranceNumber)
    {
        _insuranceNumber = insuranceNumber;
        return this;
    }

    public SalaryDecreeBuilder WithSocialSecurityContractRow(string? socialSecurityContractRow)
    {
        _socialSecurityContractRow = socialSecurityContractRow;
        return this;
    }

    public SalaryDecreeBuilder WithPositionInInsuranceList(string positionInInsuranceList)
    {
        _positionInInsuranceList = positionInInsuranceList;
        return this;
    }

    public SalaryDecreeBuilder WithIsSubjectTo7PercentInsurance(bool isSubjectTo7PercentInsurance)
    {
        _isSubjectTo7PercentInsurance = isSubjectTo7PercentInsurance;
        return this;
    }

    public SalaryDecreeBuilder WithIsSubjectTo20PercentInsurance(bool isSubjectTo20PercentInsurance)
    {
        _isSubjectTo20PercentInsurance = isSubjectTo20PercentInsurance;
        return this;
    }

    public SalaryDecreeBuilder WithIsSubjectTo3PercentInsurance(bool isSubjectTo3PercentInsurance)
    {
        _isSubjectTo3PercentInsurance = isSubjectTo3PercentInsurance;
        return this;
    }

    public SalaryDecreeBuilder WithIsSubjectTo4PercentInsurance(bool isSubjectTo4PercentInsurance)
    {
        _isSubjectTo4PercentInsurance = isSubjectTo4PercentInsurance;
        return this;
    }

    public SalaryDecreeBuilder WithInsuranceCalculationProfile(
        InsuranceCalculationProfile? insuranceCalculationProfile)
    {
        _insuranceCalculationProfile = insuranceCalculationProfile;
        return this;
    }

    public EmployeeInsuranceDto BuildInsuranceDto() =>
        new(
            _insuranceNumber,
            _socialSecurityContractRow,
            _positionInInsuranceList,
            _isSubjectTo7PercentInsurance,
            _isSubjectTo20PercentInsurance,
            _isSubjectTo3PercentInsurance,
            _isSubjectTo4PercentInsurance,
            _insuranceCalculationProfile);

    public SalaryDecreeDto BuildDto() =>
        new(
            _effectiveFrom,
            _baseDailySalary,
            _attractionAllowance,
            _supervisionAllowance,
            _shiftType,
            _contractType,
            _housingAllowance,
            _foodAllowance,
            _transportationAllowanceNet,
            _karanehAmountNet,
            _maritalStatus,
            _childrenCount,
            _isTaxSubject,
            BuildInsuranceDto());

    public DomainResult<SalaryDecree> CreateResult()
    {
        return SalaryDecree.Create(
            _id,
            _employeeId,
            _employeeHireDate,
            _latestExistingEffectiveFrom,
            _minimumMonthlySalary,
            BuildDto());
    }
}
