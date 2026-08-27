namespace Core.Tests.Domain.EmployeeSalaryProfiles;

public class UpdateEmployeeSalaryProfileTests
{
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private static EmployeeSalaryProfile CreateValidSalaryProfile()
    {
        return new EmployeeSalaryProfileBuilder()
            .WithEmployeeHireDate(ValidHireDate)
            .WithEffectiveFrom(DateOnly.FromDateTime(DateTime.Now.AddDays(-5)))
            .CreateResult()
            .ShouldBeSuccess();
    }

    private static EmployeeSalaryProfileDto BuildValidDto(DateOnly? effectiveFrom = null, decimal? baseMonthlySalary = null) =>
        new EmployeeSalaryProfileBuilder()
            .WithEffectiveFrom(effectiveFrom ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-1)))
            .WithBaseMonthlySalary(baseMonthlySalary ?? 25_000_000m)
            .BuildDto();

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var profile = CreateValidSalaryProfile();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var dto = BuildValidDto(effectiveFrom, 25_000_000m);

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            profile.EffectiveFrom.Should().Be(effectiveFrom);
            profile.BaseMonthlySalary.Should().Be(25_000_000m);
        }
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
    public void Update_WithBaseMonthlySalaryLessThanMinimum_ShouldFail()
    {
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto(baseMonthlySalary: 19_999_999m);

        var result = profile.Update(ValidHireDate, null, 20_000_000m, dto);

        result.ShouldBeFailure("حقوق پایه ماهانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithHousingAllowanceLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var profile = CreateValidSalaryProfile();
        var dto = BuildValidDto() with { HousingAllowance = amount };

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeFailure("حق مسکن");
    }

    [Fact]
    public void Update_WhenFailed_ShouldNotChangeValues()
    {
        var profile = CreateValidSalaryProfile();
        var originalEffectiveFrom = profile.EffectiveFrom;
        var originalBaseMonthlySalary = profile.BaseMonthlySalary;
        var dto = BuildValidDto(ValidHireDate.AddDays(-1));

        var result = profile.Update(ValidHireDate, null, 10_000_000m, dto);

        result.ShouldBeFailure();
        using (new AssertionScope())
        {
            profile.EffectiveFrom.Should().Be(originalEffectiveFrom);
            profile.BaseMonthlySalary.Should().Be(originalBaseMonthlySalary);
        }
    }
}
