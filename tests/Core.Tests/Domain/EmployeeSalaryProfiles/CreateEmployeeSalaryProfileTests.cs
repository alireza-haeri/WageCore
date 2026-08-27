namespace Core.Tests.Domain.EmployeeSalaryProfiles;

public class CreateEmployeeSalaryProfileTests
{
    private readonly EmployeeSalaryProfileBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.EmployeeId.Should().NotBeEmpty();
            response.BaseMonthlySalary.Should().Be(20_000_000m);
            response.AttractionAllowance.Should().BeNull();
            response.SupervisionAllowance.Should().BeNull();
            response.SeniorityBaseApplicationMode.Should().Be(SeniorityBaseApplicationMode.Manual);
            response.SeniorityBaseCalculationMethod.Should().BeNull();
            response.YearEndSeniorityMode.Should().Be(YearEndSeniorityMode.MonthlyAccrual);
            response.ShiftType.Should().Be(ShiftType.None);
            response.HousingAllowance.Should().BeNull();
            response.FoodAllowance.Should().BeNull();
            response.ChildAllowancePerChild.Should().BeNull();
            response.TransportationAllowanceNet.Should().BeNull();
            response.KaranehAmountNet.Should().BeNull();
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
            .WithBaseMonthlySalary(25_000_000m)
            .WithAttractionAllowance(1_000_000m)
            .WithSupervisionAllowance(2_000_000m)
            .WithSeniorityBaseApplicationMode(SeniorityBaseApplicationMode.Automatic)
            .WithSeniorityBaseCalculationMethod(SeniorityBaseCalculationMethod.Daily)
            .WithYearEndSeniorityMode(YearEndSeniorityMode.AnnualLumpSum)
            .WithShiftType(ShiftType.MorningEveningNight)
            .WithHousingAllowance(3_000_000m)
            .WithFoodAllowance(4_000_000m)
            .WithChildAllowancePerChild(500_000m)
            .WithTransportationAllowanceNet(800_000m)
            .WithKaranehAmountNet(1_200_000m)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.EmployeeId.Should().Be(employeeId);
            response.EffectiveFrom.Should().Be(effectiveFrom);
            response.BaseMonthlySalary.Should().Be(25_000_000m);
            response.AttractionAllowance.Should().Be(1_000_000m);
            response.SupervisionAllowance.Should().Be(2_000_000m);
            response.SeniorityBaseApplicationMode.Should().Be(SeniorityBaseApplicationMode.Automatic);
            response.SeniorityBaseCalculationMethod.Should().Be(SeniorityBaseCalculationMethod.Daily);
            response.YearEndSeniorityMode.Should().Be(YearEndSeniorityMode.AnnualLumpSum);
            response.ShiftType.Should().Be(ShiftType.MorningEveningNight);
            response.HousingAllowance.Should().Be(3_000_000m);
            response.FoodAllowance.Should().Be(4_000_000m);
            response.ChildAllowancePerChild.Should().Be(500_000m);
            response.TransportationAllowanceNet.Should().Be(800_000m);
            response.KaranehAmountNet.Should().Be(1_200_000m);
        }
    }

    [Fact]
    public void Create_WithGeneratedId_ShouldReturnSuccess()
    {
        var employeeId = Guid.NewGuid();
        var hireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var dto = new EmployeeSalaryProfileBuilder()
            .WithEffectiveFrom(hireDate)
            .BuildDto();

        var result = EmployeeSalaryProfile.Create(
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
    public void Create_WithNullBaseMonthlySalary_ShouldFail()
    {
        var result = _builder.WithBaseMonthlySalary(null).CreateResult();

        result.ShouldBeFailure("حقوق پایه ماهانه");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Create_WithBaseMonthlySalaryLessThanOrEqualToZero_ShouldFail(decimal baseMonthlySalary)
    {
        var result = _builder.WithBaseMonthlySalary(baseMonthlySalary).CreateResult();

        result.ShouldBeFailure("حقوق پایه ماهانه باید بیشتر از صفر ریال باشد.");
    }

    [Fact]
    public void Create_WithBaseMonthlySalaryLessThanMinimum_ShouldFail()
    {
        var result = _builder
            .WithMinimumMonthlySalary(20_000_000m)
            .WithBaseMonthlySalary(19_999_999m)
            .CreateResult();

        result.ShouldBeFailure("حقوق پایه ماهانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.");
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
    public void Create_WithNullSeniorityBaseApplicationMode_ShouldFail()
    {
        var result = _builder.WithSeniorityBaseApplicationMode(null).CreateResult();

        result.ShouldBeFailure("نحوه اعمال پایه سنوات");
    }

    [Fact]
    public void Create_WithAutomaticModeAndNullCalculationMethod_ShouldFail()
    {
        var result = _builder
            .WithSeniorityBaseApplicationMode(SeniorityBaseApplicationMode.Automatic)
            .WithSeniorityBaseCalculationMethod(null)
            .CreateResult();

        result.ShouldBeFailure("روش محاسبه پایه سنوات در حالت خودکار الزامی است.");
    }

    [Fact]
    public void Create_WithManualModeAndFilledCalculationMethod_ShouldFail()
    {
        var result = _builder
            .WithSeniorityBaseApplicationMode(SeniorityBaseApplicationMode.Manual)
            .WithSeniorityBaseCalculationMethod(SeniorityBaseCalculationMethod.CumulativeAuto)
            .CreateResult();

        result.ShouldBeFailure("روش محاسبه پایه سنوات در حالت دستی نباید پر شود.");
    }

    [Fact]
    public void Create_WithNullYearEndSeniorityMode_ShouldFail()
    {
        var result = _builder.WithYearEndSeniorityMode(null).CreateResult();

        result.ShouldBeFailure("نحوه محاسبه سنوات پایان سال");
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
    public void Create_WithHousingAllowanceLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithHousingAllowance(amount).CreateResult();

        result.ShouldBeFailure("حق مسکن");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithFoodAllowanceLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithFoodAllowance(amount).CreateResult();

        result.ShouldBeFailure("حق بن خواربار");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithChildAllowancePerChildLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithChildAllowancePerChild(amount).CreateResult();

        result.ShouldBeFailure("حق اولاد به ازای هر فرزند");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithTransportationAllowanceNetLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithTransportationAllowanceNet(amount).CreateResult();

        result.ShouldBeFailure("حق ایاب و ذهاب خالص");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithKaranehAmountNetLessThanOrEqualToZero_ShouldFail(decimal amount)
    {
        var result = _builder.WithKaranehAmountNet(amount).CreateResult();

        result.ShouldBeFailure("مبلغ خالص کارانه");
    }

    [Fact]
    public void Create_WithNullSalaryProfile_ShouldFail()
    {
        var result = EmployeeSalaryProfile.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
            null,
            10_000_000m,
            null);

        result.ShouldBeFailure("اطلاعات پروفایل حقوق کارمند");
    }
}
