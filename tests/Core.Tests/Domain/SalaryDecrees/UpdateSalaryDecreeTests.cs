namespace Core.Tests.Domain.SalaryDecrees;

public class UpdateSalaryDecreeTests
{
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private static SalaryDecree CreateValidSalaryProfile()
    {
        return new SalaryDecreeBuilder()
            .WithEmployeeHireDate(ValidHireDate)
            .WithEffectiveFrom(DateOnly.FromDateTime(DateTime.Now.AddDays(-5)))
            .CreateResult()
            .ShouldBeSuccess();
    }

    private static SalaryDecreeDto BuildValidDto(DateOnly? effectiveFrom = null, decimal? baseDailySalary = null) =>
        new SalaryDecreeBuilder()
            .WithEffectiveFrom(effectiveFrom ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-1)))
            .WithBaseDailySalary(baseDailySalary ?? 25_000_000m)
            .BuildDto();

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var profile = CreateValidSalaryProfile();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var dto = new SalaryDecreeBuilder()
            .WithEffectiveFrom(effectiveFrom)
            .WithBaseDailySalary(25_000_000m)
            .WithMaritalStatus(EmployeeMaritalStatus.Married)
            .WithChildrenCount(2)
            .WithIsTaxSubject(false)
            .WithInsuranceNumber("INS-777")
            .WithPositionInInsuranceList("مدیر")
            .WithIsSubjectTo7PercentInsurance(false)
            .WithIsSubjectTo20PercentInsurance(true)
            .WithIsSubjectTo3PercentInsurance(true)
            .WithIsSubjectTo4PercentInsurance(true)
            .WithInsuranceCalculationProfile(InsuranceCalculationProfile.MinimumLaborLaw)
            .BuildDto();

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            profile.EffectiveFrom.Should().Be(effectiveFrom);
            profile.BaseDailySalary.Should().Be(25_000_000m);
            profile.MaritalStatus.Should().Be(EmployeeMaritalStatus.Married);
            profile.ChildrenCount.Should().Be(2);
            profile.IsTaxSubject.Should().BeFalse();
            profile.Insurance.InsuranceNumber.Should().Be("INS-777");
            profile.Insurance.PositionInInsuranceList.Should().Be("مدیر");
            profile.Insurance.IsSubjectTo7PercentInsurance.Should().BeFalse();
            profile.Insurance.IsSubjectTo20PercentInsurance.Should().BeTrue();
            profile.Insurance.IsSubjectTo3PercentInsurance.Should().BeTrue();
            profile.Insurance.IsSubjectTo4PercentInsurance.Should().BeTrue();
            profile.Insurance.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.MinimumLaborLaw);
        }
    }

    [Fact]
    public void Update_WithInvalidInsurance_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto() with
        {
            Insurance = new EmployeeInsuranceDto(
                "INS-777",
                "",
                false,
                true,
                true,
                true,
                InsuranceCalculationProfile.MinimumLaborLaw)
        };

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeFailure("سمت در لیست بیمه");
    }

    [Fact]
    public void Update_WithNullSalaryProfile_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();

        var result = profile.Update(ValidHireDate, null, 10_000_000m, null);

        result.ShouldBeFailure("اطلاعات پروفایل حقوق کارمند");
    }

    [Fact]
    public void Update_WithNullEmployeeHireDate_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();

        var result = profile.Update(null, null, 10_000_000m, BuildValidDto());

        result.ShouldBeFailure("تاریخ استخدام کارمند");
    }

    [Fact]
    public void Update_WithNullEffectiveFrom_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto() with { EffectiveFrom = null };

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeFailure("تاریخ اعمال");
    }

    [Fact]
    public void Update_WithEffectiveFromBeforeHireDate_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto(ValidHireDate.AddDays(-1));

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از تاریخ استخدام کارمند باشد.");
    }

    [Fact]
    public void Update_WithEffectiveFromEqualToLatestExisting_ShouldFail()
    {
        var latestExisting = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto(latestExisting);

        var result = profile.Update(ValidHireDate, latestExisting, 10_000_000m, dto);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از پروفایل حقوق قبلی کارمند باشد.");
    }

    [Fact]
    public void Update_WithEffectiveFromBeforeLatestExisting_ShouldFail()
    {
        var latestExisting = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto(latestExisting.AddDays(-1));

        var result = profile.Update(ValidHireDate, latestExisting, 10_000_000m, dto);

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از پروفایل حقوق قبلی کارمند باشد.");
    }

    [Fact]
    public void Update_WithNullMinimumMonthlySalary_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();

        var result = profile.Update(ValidHireDate, null, null, BuildValidDto());

        result.ShouldBeFailure("حداقل حقوق ماهانه");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithMinimumMonthlySalaryLessThanOrEqualToZero_ShouldFail(decimal minimumMonthlySalary)
    {
        var profile = CreateValidSalaryProfile();

        var result = profile.Update(ValidHireDate, null, minimumMonthlySalary, BuildValidDto());

        result.ShouldBeFailure("حداقل حقوق ماهانه باید بیشتر از صفر ریال باشد.");
    }

    [Fact]
    public void Update_WithBaseDailySalaryLessThanMinimum_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto(baseDailySalary: 19_999_999m);

        var result = profile.Update(ValidHireDate, null, 20_000_000m, dto);

        result.ShouldBeFailure("حقوق پایه روزانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.");
    }
    [Fact]
    public void Update_WhenFailed_ShouldNotChangeValues()
    {
        var profile = CreateValidSalaryProfile();
        var originalEffectiveFrom = profile.EffectiveFrom;
        var originalBaseDailySalary = profile.BaseDailySalary;
        var dto = BuildValidDto(ValidHireDate.AddDays(-1));

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeFailure();
        using (new AssertionScope())
        {
            profile.EffectiveFrom.Should().Be(originalEffectiveFrom);
            profile.BaseDailySalary.Should().Be(originalBaseDailySalary);
        }
    }
}
