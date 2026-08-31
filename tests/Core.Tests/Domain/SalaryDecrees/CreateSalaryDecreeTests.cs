namespace Core.Tests.Domain.SalaryDecrees;

public class CreateSalaryDecreeTests
{
    private readonly SalaryDecreeBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.EmployeeId.Should().NotBeEmpty();
            response.BaseDailySalary.Should().Be(20_000_000m);
            response.AttractionAllowance.Should().BeNull();
            response.SupervisionAllowance.Should().BeNull();
            response.ShiftType.Should().Be(ShiftType.None);
            response.ContractType.Should().Be(ContractType.Permanent);
            response.TransportationAllowanceNet.Should().BeNull();
            response.MaritalStatus.Should().Be(EmployeeMaritalStatus.Single);
            response.ChildrenCount.Should().Be(0);
            response.IsTaxSubject.Should().BeTrue();
            response.Insurance.InsuranceNumber.Should().Be("INS-001");
            response.Insurance.PositionInInsuranceList.Should().Be("اپراتور");
            response.Insurance.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.FullLegal);
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-60));
        var latestExistingEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-20));
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var result = _builder
            .WithId(id)
            .WithEmployeeId(employeeId)
            .WithEmployeeHireDate(hireDate)
            .WithLatestExistingEffectiveFrom(latestExistingEffectiveFrom)
            .WithMinimumMonthlySalary(15_000_000m)
            .WithEffectiveFrom(effectiveFrom)
            .WithBaseDailySalary(25_000_000m)
            .WithAttractionAllowance(1_000_000m)
            .WithSupervisionAllowance(2_000_000m)
            .WithShiftType(ShiftType.MorningEveningNight)
            .WithContractType(ContractType.FixedTerm)
            .WithTransportationAllowanceNet(800_000m)
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
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.EmployeeId.Should().Be(employeeId);
            response.EffectiveFrom.Should().Be(effectiveFrom);
            response.BaseDailySalary.Should().Be(25_000_000m);
            response.AttractionAllowance.Should().Be(1_000_000m);
            response.SupervisionAllowance.Should().Be(2_000_000m);
            response.ShiftType.Should().Be(ShiftType.MorningEveningNight);
            response.ContractType.Should().Be(ContractType.FixedTerm);
            response.TransportationAllowanceNet.Should().Be(800_000m);
            response.MaritalStatus.Should().Be(EmployeeMaritalStatus.Married);
            response.ChildrenCount.Should().Be(2);
            response.IsTaxSubject.Should().BeFalse();
            response.Insurance.InsuranceNumber.Should().Be("INS-777");
            response.Insurance.PositionInInsuranceList.Should().Be("مدیر");
            response.Insurance.IsSubjectTo7PercentInsurance.Should().BeFalse();
            response.Insurance.IsSubjectTo20PercentInsurance.Should().BeTrue();
            response.Insurance.IsSubjectTo3PercentInsurance.Should().BeTrue();
            response.Insurance.IsSubjectTo4PercentInsurance.Should().BeTrue();
            response.Insurance.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.MinimumLaborLaw);
        }
    }

    [Fact]
    public void Create_WithGeneratedId_ShouldReturnSuccess()
    {
        var employeeId = Guid.NewGuid();
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var dto = new SalaryDecreeBuilder()
            .WithEffectiveFrom(hireDate)
            .BuildDto();

        var result = SalaryDecree.Create(
            employeeId,
            hireDate,
            null,
            10_000_000m,
            dto);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.EmployeeId.Should().Be(employeeId);
        }
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه پروفایل حقوق کارمند");
    }

    [Fact]
    public void Create_WithEmptyEmployeeId_ShouldFail()
    {
        var result = _builder.WithEmployeeId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه کارمند");
    }

    [Fact]
    public void Create_WithNullEmployeeHireDate_ShouldFail()
    {
        var result = _builder.WithEmployeeHireDate(null).CreateResult();

        result.ShouldBeFailure("تاریخ استخدام کارمند");
    }

    [Fact]
    public void Create_WithNullEffectiveFrom_ShouldFail()
    {
        var result = _builder.WithEffectiveFrom(null).CreateResult();

        result.ShouldBeFailure("تاریخ اعمال");
    }

    [Fact]
    public void Create_WithEffectiveFromBeforeHireDate_ShouldFail()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var result = _builder
            .WithEmployeeHireDate(hireDate)
            .WithEffectiveFrom(hireDate.AddDays(-1))
            .CreateResult();

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از تاریخ استخدام کارمند باشد.");
    }

    [Fact]
    public void Create_WithEffectiveFromBeforeExistingProfile_ShouldFail()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var latestExisting = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
        var result = _builder
            .WithEmployeeHireDate(hireDate)
            .WithLatestExistingEffectiveFrom(latestExisting)
            .WithEffectiveFrom(latestExisting.AddDays(-1))
            .CreateResult();

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از پروفایل حقوق قبلی کارمند باشد.");
    }

    [Fact]
    public void Create_WithEffectiveFromEqualToExistingProfile_ShouldFail()
    {
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var latestExisting = DateOnly.FromDateTime(DateTime.Now.AddDays(-5));
        var result = _builder
            .WithEmployeeHireDate(hireDate)
            .WithLatestExistingEffectiveFrom(latestExisting)
            .WithEffectiveFrom(latestExisting)
            .CreateResult();

        result.ShouldBeFailure("تاریخ اعمال نباید قبل از پروفایل حقوق قبلی کارمند باشد.");
    }

    [Fact]
    public void Create_WithNullMinimumMonthlySalary_ShouldFail()
    {
        var result = _builder.WithMinimumMonthlySalary(null).CreateResult();

        result.ShouldBeFailure("حداقل حقوق ماهانه");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithMinimumMonthlySalaryLessThanOrEqualToZero_ShouldFail(decimal minimumMonthlySalary)
    {
        var result = _builder.WithMinimumMonthlySalary(minimumMonthlySalary).CreateResult();

        result.ShouldBeFailure("حداقل حقوق ماهانه باید بیشتر از صفر ریال باشد.");
    }

    [Fact]
    public void Create_WithNullBaseDailySalary_ShouldFail()
    {
        var result = _builder.WithBaseDailySalary(null).CreateResult();

        result.ShouldBeFailure("حقوق پایه روزانه");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Create_WithBaseDailySalaryLessThanOrEqualToZero_ShouldFail(decimal baseDailySalary)
    {
        var result = _builder.WithBaseDailySalary(baseDailySalary).CreateResult();

        result.ShouldBeFailure("حقوق پایه روزانه باید بیشتر از صفر ریال باشد.");
    }

    [Fact]
    public void Create_WithBaseDailySalaryLessThanMinimum_ShouldFail()
    {
        var result = _builder
            .WithMinimumMonthlySalary(20_000_000m)
            .WithBaseDailySalary(19_999_999m)
            .CreateResult();

        result.ShouldBeFailure("حقوق پایه روزانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithAttractionAllowanceLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithAttractionAllowance(amount).CreateResult();

        result.ShouldBeFailure("حق جذب");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithSupervisionAllowanceLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithSupervisionAllowance(amount).CreateResult();

        result.ShouldBeFailure("حق سرپرستی");
    }

    [Fact]
    public void Create_WithNullContractType_ShouldFail()
    {
        var result = _builder.WithContractType(null).CreateResult();

        result.ShouldBeFailure("نوع قرارداد");
    }

    [Fact]
    public void Create_WithInvalidContractType_ShouldFail()
    {
        var result = _builder.WithContractType((ContractType)999).CreateResult();

        result.ShouldBeFailure("نوع قرارداد معتبر نیست.");
    }

    [Fact]
    public void Create_WithNullShiftType_ShouldFail()
    {
        var result = _builder.WithShiftType(null).CreateResult();

        result.ShouldBeFailure("نوع شیفت");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithTransportationAllowanceNetLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithTransportationAllowanceNet(amount).CreateResult();

        result.ShouldBeFailure("حق ایاب و ذهاب خالص");
    }
    [Fact]
    public void Create_WithNullMaritalStatus_ShouldFail()
    {
        var result = _builder.WithMaritalStatus(null).CreateResult();

        result.ShouldBeFailure("وضعیت تاهل");
    }

    [Fact]
    public void Create_WithInvalidMaritalStatus_ShouldFail()
    {
        var result = _builder.WithMaritalStatus((EmployeeMaritalStatus)999).CreateResult();

        result.ShouldBeFailure("وضعیت تاهل معتبر نیست.");
    }

    [Fact]
    public void Create_WithNullChildrenCount_ShouldFail()
    {
        var result = _builder.WithChildrenCount(null).CreateResult();

        result.ShouldBeFailure("تعداد فرزندان");
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
    public void Create_WithSingleMaritalStatusAndChildrenCountMoreThanZero_ShouldFail()
    {
        var result = _builder
            .WithMaritalStatus(EmployeeMaritalStatus.Single)
            .WithChildrenCount(1)
            .CreateResult();

        result.ShouldBeFailure("برای کارمند مجرد، تعداد فرزندان باید صفر باشد.");
    }

    [Fact]
    public void Create_WithNullIsTaxSubject_ShouldFail()
    {
        var result = _builder.WithIsTaxSubject(null).CreateResult();

        result.ShouldBeFailure("مشمول مالیات");
    }

    [Fact]
    public void Create_WithInvalidInsurance_ShouldFail()
    {
        var result = _builder
            .WithPositionInInsuranceList("")
            .CreateResult();

        result.ShouldBeFailure("سمت در لیست بیمه");
    }

    [Fact]
    public void Create_WithNullSalaryProfile_ShouldFail()
    {
        var result = SalaryDecree.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
            null,
            10_000_000m,
            null);

        result.ShouldBeFailure("اطلاعات پروفایل حقوق کارمند");
    }
}
