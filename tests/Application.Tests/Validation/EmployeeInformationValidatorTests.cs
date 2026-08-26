namespace Application.Tests.Validation;

public class EmployeeInformationValidatorTests
{
    private readonly EmployeeInformationValidator _validator = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    [Fact]
    public void Validate_WithValidDto_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDepartmentIdEmpty_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { DepartmentId = Guid.Empty };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void Validate_WithPersonalCodeExactly20Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { PersonalCode = "AB123456789012345678" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.PersonalCode);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    [InlineData("A-100")]
    [InlineData("۱۲۳")]
    [InlineData("ABC 123")]
    public void Validate_WithInvalidPersonalCode_ShouldHaveValidationError(string personalCode)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { PersonalCode = personalCode };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PersonalCode);
    }

    [Fact]
    public void Validate_WithFullNameExactly3Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { FullName = "علیا" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WithFullNameExactly100Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { FullName = new string('a', 100) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    [InlineData("عل")]
    public void Validate_WithInvalidFullName_ShouldHaveValidationError(string fullName)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { FullName = fullName };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WithFullNameMoreThan100Characters_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { FullName = new string('a', 101) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Validate_WithInvalidNationalCode_ShouldHaveValidationError(string nationalCode)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { NationalCode = nationalCode };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NationalCode);
    }

    [Fact]
    public void Validate_WithValidNationalCode_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { NationalCode = "0987654321" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.NationalCode);
    }

    [Theory]
    [InlineData("12345A")]
    [InlineData("۱۲۳۴۵")]
    public void Validate_WithInvalidBirthCertificateNumber_ShouldHaveValidationError(string birthCertificateNumber)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { BirthCertificateNumber = birthCertificateNumber };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BirthCertificateNumber);
    }

    [Fact]
    public void Validate_WithBirthCertificateNumberExactly20Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { BirthCertificateNumber = "12345678901234567890" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.BirthCertificateNumber);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    [InlineData("آب")]
    public void Validate_WithInvalidFatherName_ShouldHaveValidationError(string fatherName)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { FatherName = fatherName };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FatherName);
    }

    [Fact]
    public void Validate_WithFatherNameExactly50Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { FatherName = new string('a', 50) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.FatherName);
    }

    [Fact]
    public void Validate_WithFatherNameMoreThan50Characters_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { FatherName = new string('a', 51) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FatherName);
    }

    [Fact]
    public void Validate_WithNullGender_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { Gender = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Validate_WithInvalidGender_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { Gender = (EmployeeGender)999 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Validate_WithNullMaritalStatus_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { MaritalStatus = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus);
    }

    [Fact]
    public void Validate_WithInvalidMaritalStatus_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { MaritalStatus = (EmployeeMaritalStatus)999 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus);
    }

    [Fact]
    public void Validate_WithNullChildrenCount_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { ChildrenCount = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Validate_WithChildrenCountOutOfRange_ShouldHaveValidationError(int childrenCount)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { ChildrenCount = childrenCount };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    public void Validate_WithChildrenCountBoundary_ShouldNotHaveAnyErrors(int childrenCount)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { ChildrenCount = childrenCount };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Fact]
    public void Validate_WithSingleMaritalStatusAndChildrenCountMoreThanZero_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with
        {
            MaritalStatus = EmployeeMaritalStatus.Single,
            ChildrenCount = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Fact]
    public void Validate_WithNullHireDate_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { HireDate = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.HireDate);
    }

    [Fact]
    public void Validate_WithHireDateInFuture_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { HireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.HireDate);
    }

    [Fact]
    public void Validate_WithHireDateToday_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { HireDate = DateOnly.FromDateTime(DateTime.Now) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.HireDate);
    }

    [Theory]
    [MemberData(nameof(PhoneNumberTestData.InvalidLengthPhoneNumbers), MemberType = typeof(PhoneNumberTestData))]
    [MemberData(nameof(PhoneNumberTestData.NoneEnglishDigitsOrInvalidCharacters), MemberType = typeof(PhoneNumberTestData))]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveValidationError(string phoneNumber)
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { PhoneNumber = phoneNumber };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { PhoneNumber = "09987654321" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithNullJobTitle_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { JobTitle = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.JobTitle);
    }

    [Fact]
    public void Validate_WithJobTitleExactly100Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { JobTitle = new string('a', 100) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.JobTitle);
    }

    [Fact]
    public void Validate_WithJobTitleMoreThan100Characters_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildEmployeeDto() with { JobTitle = new string('a', 101) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.JobTitle);
    }
}
