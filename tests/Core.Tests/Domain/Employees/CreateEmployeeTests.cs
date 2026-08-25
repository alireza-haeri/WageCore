namespace Core.Tests.Domain.Employees;

public class CreateEmployeeTests
{
    private readonly EmployeeBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.WorkshopId.Should().NotBeEmpty();
            response.DepartmentId.Should().NotBeEmpty();
            response.PersonalCode.Should().Be("EMP001");
            response.FullName.Should().Be("کارمند نمونه");
            response.NationalCode.Should().Be("1234567890");
            response.BirthCertificateNumber.Should().Be("12345");
            response.FatherName.Should().Be("محمد");
            response.Gender.Should().Be(EmployeeGender.Man);
            response.MaritalStatus.Should().Be(EmployeeMaritalStatus.Single);
            response.ChildrenCount.Should().Be(0);
            response.PhoneNumber.Should().Be("09123456789");
            response.JobTitle.Should().Be("حسابدار");
            response.IsTaxSubject.Should().BeTrue();
            response.TerminationDate.Should().BeNull();
            response.BankAccounts.Should().BeEmpty();
            response.Insurance.InsuranceNumber.Should().Be("INS-001");
            response.Insurance.SocialSecurityContractRow.Should().Be("CTR-10");
            response.Insurance.PositionInInsuranceList.Should().Be("اپراتور");
            response.Insurance.IsSubjectTo7PercentInsurance.Should().BeTrue();
            response.Insurance.IsSubjectTo20PercentInsurance.Should().BeTrue();
            response.Insurance.IsSubjectTo3PercentInsurance.Should().BeFalse();
            response.Insurance.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.FullLegal);
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var workshopId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var workshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-60));
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var result = _builder
            .WithId(id)
            .WithWorkshopId(workshopId)
            .WithDepartmentId(departmentId)
            .WithPersonalCode("A123456")
            .WithFullName("علی رضایی")
            .WithNationalCode("0987654321")
            .WithBirthCertificateNumber("67890")
            .WithFatherName("حسین")
            .WithGender(EmployeeGender.Woman)
            .WithMaritalStatus(EmployeeMaritalStatus.Married)
            .WithChildrenCount(2)
            .WithWorkshopRegistrationDate(workshopRegistrationDate)
            .WithHireDate(hireDate)
            .WithPhoneNumber("09987654321")
            .WithJobTitle("سرپرست")
            .WithIsTaxSubject(false)
            .WithInsuranceNumber("INS-999")
            .WithSocialSecurityContractRow(null)
            .WithPositionInInsuranceList("مدیر تولید")
            .WithIsSubjectTo7PercentInsurance(false)
            .WithIsSubjectTo20PercentInsurance(true)
            .WithIsSubjectTo3PercentInsurance(true)
            .WithInsuranceCalculationProfile(InsuranceCalculationProfile.MinimumLaborLaw)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.WorkshopId.Should().Be(workshopId);
            response.DepartmentId.Should().Be(departmentId);
            response.PersonalCode.Should().Be("A123456");
            response.FullName.Should().Be("علی رضایی");
            response.NationalCode.Should().Be("0987654321");
            response.BirthCertificateNumber.Should().Be("67890");
            response.FatherName.Should().Be("حسین");
            response.Gender.Should().Be(EmployeeGender.Woman);
            response.MaritalStatus.Should().Be(EmployeeMaritalStatus.Married);
            response.ChildrenCount.Should().Be(2);
            response.HireDate.Should().Be(hireDate);
            response.PhoneNumber.Should().Be("09987654321");
            response.JobTitle.Should().Be("سرپرست");
            response.IsTaxSubject.Should().BeFalse();
            response.Insurance.InsuranceNumber.Should().Be("INS-999");
            response.Insurance.SocialSecurityContractRow.Should().BeNull();
            response.Insurance.PositionInInsuranceList.Should().Be("مدیر تولید");
            response.Insurance.IsSubjectTo7PercentInsurance.Should().BeFalse();
            response.Insurance.IsSubjectTo20PercentInsurance.Should().BeTrue();
            response.Insurance.IsSubjectTo3PercentInsurance.Should().BeTrue();
            response.Insurance.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.MinimumLaborLaw);
        }
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه کارمند");
    }

    [Fact]
    public void Create_WithEmptyWorkshopId_ShouldFail()
    {
        var result = _builder.WithWorkshopId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه کارگاه");
    }

    [Fact]
    public void Create_WithEmptyDepartmentId_ShouldFail()
    {
        var result = _builder.WithDepartmentId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه بخش");
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWhiteSpacePersonalCode_ShouldFail(string? personalCode)
    {
        var result = _builder.WithPersonalCode(personalCode!).CreateResult();

        result.ShouldBeFailure("کد پرسنلی");
    }

    [Theory]
    [InlineData("A-100")]
    [InlineData("۱۲۳")]
    [InlineData("ABC 123")]
    public void Create_WithInvalidPersonalCode_ShouldFail(string personalCode)
    {
        var result = _builder.WithPersonalCode(personalCode).CreateResult();

        result.ShouldBeFailure("کد پرسنلی باید بین 1 تا 20 کاراکتر و فقط شامل حروف و اعداد انگلیسی باشد.");
    }

    [Fact]
    public void Create_WithDuplicatePersonalCodeInWorkshop_ShouldFail()
    {
        var result = _builder.WithPersonalCodeUniqueInWorkshop(false).CreateResult();

        result.ShouldBeFailure("کد پرسنلی در این کارگاه تکراری است.");
    }

    [Theory]
    [InlineData("عل")]
    [InlineData("آب")]
    public void Create_WithFullNameLessThan3Characters_ShouldFail(string fullName)
    {
        var result = _builder.WithFullName(fullName).CreateResult();

        result.ShouldBeFailure("نام و نام خانوادگی باید بین 3 تا 100 حرف باشد.");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Create_WithInvalidNationalCode_ShouldFail(string nationalCode)
    {
        var result = _builder.WithNationalCode(nationalCode).CreateResult();

        result.ShouldBeFailure("کد ملی باید 10 رقم انگلیسی باشد.");
    }

    [Fact]
    public void Create_WithDuplicateNationalCodeForUser_ShouldFail()
    {
        var result = _builder.WithNationalCodeUniqueForUser(false).CreateResult();

        result.ShouldBeFailure("کد ملی در بین کارکنان این کاربر تکراری است.");
    }

    [Theory]
    [InlineData("12345A")]
    [InlineData("۱۲۳۴۵")]
    public void Create_WithInvalidBirthCertificateNumber_ShouldFail(string birthCertificateNumber)
    {
        var result = _builder.WithBirthCertificateNumber(birthCertificateNumber).CreateResult();

        result.ShouldBeFailure("شماره شناسنامه باید بین 1 تا 20 رقم انگلیسی باشد.");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("آب")]
    public void Create_WithFatherNameLessThan3Characters_ShouldFail(string fatherName)
    {
        var result = _builder.WithFatherName(fatherName).CreateResult();

        result.ShouldBeFailure("نام پدر باید بین 3 تا 50 حرف باشد.");
    }

    [Fact]
    public void Create_WithNullGender_ShouldFail()
    {
        var result = _builder.WithGender(null).CreateResult();

        result.ShouldBeFailure("جنسیت");
    }

    [Fact]
    public void Create_WithNullMaritalStatus_ShouldFail()
    {
        var result = _builder.WithMaritalStatus(null).CreateResult();

        result.ShouldBeFailure("وضعیت تاهل");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Create_WithChildrenCountOutOfRange_ShouldFail(int childrenCount)
    {
        var result = _builder.WithChildrenCount(childrenCount).CreateResult();

        result.ShouldBeFailure("تعداد فرزندان باید بین 0 تا 20 باشد.");
    }

    [Fact]
    public void Create_WithHireDateInFuture_ShouldFail()
    {
        var result = _builder.WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))).CreateResult();

        result.ShouldBeFailure("تاریخ استخدام نباید برای آینده باشد.");
    }

    [Fact]
    public void Create_WithHireDateBeforeWorkshopRegistrationDate_ShouldFail()
    {
        var result = _builder
            .WithWorkshopRegistrationDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-10)))
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-11)))
            .CreateResult();

        result.ShouldBeFailure("تاریخ استخدام نباید قبل از تاریخ ثبت کارگاه باشد.");
    }

    [Theory]
    [MemberData(nameof(PhoneNumberTestData.InvalidLengthPhoneNumbers), MemberType = typeof(PhoneNumberTestData))]
    [MemberData(nameof(PhoneNumberTestData.NoneEnglishDigitsOrInvalidCharacters), MemberType = typeof(PhoneNumberTestData))]
    public void Create_WithInvalidPhoneNumber_ShouldFail(string phoneNumber)
    {
        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();

        result.ShouldBeFailure("شماره تلفن باید با ۰۹ شروع شده و دقیقاً ۱۱ رقم انگلیسی باشد.");
    }

    [Fact]
    public void Create_WithWhiteSpaceJobTitle_ShouldReturnSuccessWithNullJobTitle()
    {
        var result = _builder.WithJobTitle("   ").CreateResult();

        var response = result.ShouldBeSuccess();
        response.JobTitle.Should().BeNull();
    }

    [Fact]
    public void Create_WithJobTitleMoreThan100Characters_ShouldFail()
    {
        var result = _builder.WithJobTitle(new string('a', 101)).CreateResult();

        result.ShouldBeFailure("عنوان شغلی نمیتواند بیشتر از 100 حرف باشد.");
    }

    [Fact]
    public void Create_WithInvalidInsurance_ShouldFail()
    {
        var result = _builder.WithInsuranceNumber("").CreateResult();

        result.ShouldBeFailure("شماره بیمه");
    }
}
