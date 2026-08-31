namespace Shared.Tests.Builders;

public class EmployeeBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _workshopId = Guid.NewGuid();
    private Guid _departmentId = Guid.NewGuid();
    private string _personalCode = "EMP001";
    private string _fullName = "کارمند نمونه";
    private string _nationalCode = "1234567890";
    private string _fatherName = "محمد";
    private EmployeeGender? _gender = EmployeeGender.Man;
    private DateOnly? _workshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private DateOnly? _hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
    private string _phoneNumber = "09123456789";
    private string? _jobTitle = "حسابدار";
    private Region? _region = Region.Normal;
    private string _insuranceNumber = "INS-001";
    private string? _socialSecurityContractRow = "CTR-10";
    private string _positionInInsuranceList = "اپراتور";
    private bool _isSubjectTo7PercentInsurance = true;
    private bool _isSubjectTo20PercentInsurance = true;
    private bool _isSubjectTo3PercentInsurance = false;
    private bool _isSubjectTo4PercentInsurance = false;
    private InsuranceCalculationProfile? _insuranceCalculationProfile = InsuranceCalculationProfile.FullLegal;
    private string? _bankName = "بانک ملی";
    private string? _branchCode = "۱۰۲";
    private string _iban = "IR123456789012345678901234";
    private bool _isPersonalCodeUniqueForUser = true;
    private bool _isNationalCodeUniqueForUser = true;

    public EmployeeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EmployeeBuilder WithWorkshopId(Guid workshopId)
    {
        _workshopId = workshopId;
        return this;
    }

    public EmployeeBuilder WithDepartmentId(Guid departmentId)
    {
        _departmentId = departmentId;
        return this;
    }

    public EmployeeBuilder WithPersonalCode(string personalCode)
    {
        _personalCode = personalCode;
        return this;
    }

    public EmployeeBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public EmployeeBuilder WithNationalCode(string nationalCode)
    {
        _nationalCode = nationalCode;
        return this;
    }

    public EmployeeBuilder WithFatherName(string fatherName)
    {
        _fatherName = fatherName;
        return this;
    }

    public EmployeeBuilder WithGender(EmployeeGender? gender)
    {
        _gender = gender;
        return this;
    }

    public EmployeeBuilder WithWorkshopRegistrationDate(DateOnly? workshopRegistrationDate)
    {
        _workshopRegistrationDate = workshopRegistrationDate;
        return this;
    }

    public EmployeeBuilder WithHireDate(DateOnly? hireDate)
    {
        _hireDate = hireDate;
        return this;
    }

    public EmployeeBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public EmployeeBuilder WithJobTitle(string? jobTitle)
    {
        _jobTitle = jobTitle;
        return this;
    }

    public EmployeeBuilder WithRegion(Region? region)
    {
        _region = region;
        return this;
    }

    public EmployeeBuilder WithInsuranceNumber(string insuranceNumber)
    {
        _insuranceNumber = insuranceNumber;
        return this;
    }

    public EmployeeBuilder WithSocialSecurityContractRow(string? socialSecurityContractRow)
    {
        _socialSecurityContractRow = socialSecurityContractRow;
        return this;
    }

    public EmployeeBuilder WithPositionInInsuranceList(string positionInInsuranceList)
    {
        _positionInInsuranceList = positionInInsuranceList;
        return this;
    }

    public EmployeeBuilder WithIsSubjectTo7PercentInsurance(bool isSubjectTo7PercentInsurance)
    {
        _isSubjectTo7PercentInsurance = isSubjectTo7PercentInsurance;
        return this;
    }

    public EmployeeBuilder WithIsSubjectTo20PercentInsurance(bool isSubjectTo20PercentInsurance)
    {
        _isSubjectTo20PercentInsurance = isSubjectTo20PercentInsurance;
        return this;
    }

    public EmployeeBuilder WithIsSubjectTo3PercentInsurance(bool isSubjectTo3PercentInsurance)
    {
        _isSubjectTo3PercentInsurance = isSubjectTo3PercentInsurance;
        return this;
    }

    public EmployeeBuilder WithIsSubjectTo4PercentInsurance(bool isSubjectTo4PercentInsurance)
    {
        _isSubjectTo4PercentInsurance = isSubjectTo4PercentInsurance;
        return this;
    }

    public EmployeeBuilder WithInsuranceCalculationProfile(InsuranceCalculationProfile? insuranceCalculationProfile)
    {
        _insuranceCalculationProfile = insuranceCalculationProfile;
        return this;
    }

    public EmployeeBuilder WithBankName(string? bankName)
    {
        _bankName = bankName;
        return this;
    }

    public EmployeeBuilder WithBranchCode(string? branchCode)
    {
        _branchCode = branchCode;
        return this;
    }

    public EmployeeBuilder WithIban(string iban)
    {
        _iban = iban;
        return this;
    }

    public EmployeeBuilder WithPersonalCodeUniqueForUser(bool isPersonalCodeUniqueForUser)
    {
        _isPersonalCodeUniqueForUser = isPersonalCodeUniqueForUser;
        return this;
    }

    public EmployeeBuilder WithNationalCodeUniqueForUser(bool isNationalCodeUniqueForUser)
    {
        _isNationalCodeUniqueForUser = isNationalCodeUniqueForUser;
        return this;
    }

    public EmployeeDto BuildEmployeeDto() =>
        new(
            _departmentId,
            _personalCode,
            _fullName,
            _nationalCode,
            _fatherName,
            _gender,
            _hireDate,
            _phoneNumber,
            _jobTitle,
            _region);

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

    public EmployeeBankAccountDto BuildBankAccountDto() =>
        new(_bankName, _branchCode, _iban);

    public DomainResult<Employee> CreateResult()
    {
        return Employee.Create(
            _id,
            _workshopId,
            _workshopRegistrationDate,
            BuildEmployeeDto(),
            _isPersonalCodeUniqueForUser,
            _isNationalCodeUniqueForUser);
    }
}
