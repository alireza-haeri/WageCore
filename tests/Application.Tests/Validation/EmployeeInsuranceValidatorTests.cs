namespace Application.Tests.Validation;

public class EmployeeInsuranceValidatorTests
{
    private readonly EmployeeInsuranceValidator _validator = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    [Fact]
    public void Validate_WithValidDto_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildInsuranceDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInsuranceNumberExactly20Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { InsuranceNumber = new string('a', 20) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.InsuranceNumber);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithInvalidInsuranceNumber_ShouldHaveValidationError(string? insuranceNumber)
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { InsuranceNumber = insuranceNumber! };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.InsuranceNumber);
    }

    [Fact]
    public void Validate_WithInsuranceNumberMoreThan20Characters_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { InsuranceNumber = new string('a', 21) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.InsuranceNumber);
    }

    [Fact]
    public void Validate_WithNullSocialSecurityContractRow_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { SocialSecurityContractRow = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.SocialSecurityContractRow);
    }

    [Fact]
    public void Validate_WithSocialSecurityContractRowMoreThan20Characters_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { SocialSecurityContractRow = new string('a', 21) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SocialSecurityContractRow);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithInvalidPositionInInsuranceList_ShouldHaveValidationError(string? positionInInsuranceList)
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { PositionInInsuranceList = positionInInsuranceList! };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PositionInInsuranceList);
    }

    [Fact]
    public void Validate_WithPositionInInsuranceListExactly100Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { PositionInInsuranceList = new string('a', 100) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.PositionInInsuranceList);
    }

    [Fact]
    public void Validate_WithPositionInInsuranceListMoreThan100Characters_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { PositionInInsuranceList = new string('a', 101) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PositionInInsuranceList);
    }

    [Fact]
    public void Validate_WithNullInsuranceCalculationProfile_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with { InsuranceCalculationProfile = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.InsuranceCalculationProfile);
    }

    [Fact]
    public void Validate_WithInvalidInsuranceCalculationProfile_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildInsuranceDto() with
        {
            InsuranceCalculationProfile = (InsuranceCalculationProfile)999
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.InsuranceCalculationProfile);
    }
}
