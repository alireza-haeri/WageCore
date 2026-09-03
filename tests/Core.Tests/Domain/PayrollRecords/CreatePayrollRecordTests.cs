namespace Core.Tests.Domain.PayrollRecords;

public class CreatePayrollRecordTests
{
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);

    private readonly PayrollRecordBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.EmployeeId.Should().NotBeEmpty();
            response.WorkedDaysCount.Should().Be(24m);
            response.OvertimeHours.Should().Be(4m);
            response.NightShiftHours.Should().Be(3m);
            response.FridayWorkHours.Should().Be(2m);
            response.LeaveHours.Should().Be(2m);
            response.AbsenceDaysCount.Should().Be(0m);
            response.MissionDaysCount.Should().Be(1m);
            response.MissionHours.Should().Be(0m);
            response.HolidayWorkHours.Should().Be(0m);
            response.MissionAmountOverride.Should().BeNull();
            response.StandardWorkingDaysCount.Should().Be(31);
            response.IsEsfandPeriod.Should().BeFalse();
            response.AnnualBonusType.Should().BeNull();
            response.PerformanceBonusAmount.Should().BeNull();
            response.CashBenefitsAmount.Should().BeNull();
            response.OvertimeAmount.Should().Be(800_000m);
            response.NightShiftExtraAmount.Should().Be(300_000m);
            response.FridayWorkAllowance.Should().Be(250_000m);
            response.CalculatedTaxAmount.Should().Be(1_500_000m);
            response.GrossAmount.Should().Be(17_900_000m);
            response.InsuranceAmount.Should().Be(1_400_000m);
            response.TotalDeductionsAmount.Should().Be(2_900_000m);
            response.NetPayableAmount.Should().Be(15_000_000m);
            response.Status.Should().Be(PayrollRecordStatus.Draft);
            response.IsPaid.Should().BeFalse();
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var result = _builder
            .WithId(id)
            .WithEmployeeId(employeeId)
            .WithEmployeeIsTaxSubject(true)
            .WithPeriod(PeriodStart, PeriodEnd)
            .WithMaxMonthlyOvertimeHours(36m)
            .WithMaxFridayHours(24m)
            .WithWorkedDaysCount(26.5m)
            .WithOvertimeHours(36m)
            .WithNightShiftHours(7.25m)
            .WithFridayWorkHours(24m)
            .WithLeaveHours(2.5m)
            .WithAbsenceDaysCount(1.5m)
            .WithMissionDaysCount(3.75m)
            .WithMissionHours(4m)
            .WithHolidayWorkHours(3m)
            .WithMissionAmountOverride(500_000m)
            .WithStandardWorkingDaysCount(30)
            .WithIsEsfandPeriod(true)
            .WithAnnualBonusType(AnnualBonusType.Maximum)
            .WithPerformanceBonusAmount(2_000_000m)
            .WithCashBenefitsAmount(300_000m)
            .WithOvertimeAmount(1_200_000m)
            .WithNightShiftExtraAmount(450_000m)
            .WithFridayWorkAllowance(375_000m)
            .WithCalculatedTaxAmount(4_500_000m)
            .WithGrossAmount(25_000_000m)
            .WithInsuranceAmount(1_750_000m)
            .WithTotalDeductionsAmount(6_250_000m)
            .WithNetPayableAmount(18_750_000m)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.EmployeeId.Should().Be(employeeId);
            response.PeriodStart.Should().Be(PeriodStart);
            response.PeriodEnd.Should().Be(PeriodEnd);
            response.WorkedDaysCount.Should().Be(26.5m);
            response.OvertimeHours.Should().Be(36m);
            response.NightShiftHours.Should().Be(7.25m);
            response.FridayWorkHours.Should().Be(24m);
            response.LeaveHours.Should().Be(2.5m);
            response.AbsenceDaysCount.Should().Be(1.5m);
            response.MissionDaysCount.Should().Be(3.75m);
            response.MissionHours.Should().Be(4m);
            response.HolidayWorkHours.Should().Be(3m);
            response.MissionAmountOverride.Should().Be(500_000m);
            response.StandardWorkingDaysCount.Should().Be(30);
            response.IsEsfandPeriod.Should().BeTrue();
            response.AnnualBonusType.Should().Be(AnnualBonusType.Maximum);
            response.PerformanceBonusAmount.Should().Be(2_000_000m);
            response.CashBenefitsAmount.Should().Be(300_000m);
            response.OvertimeAmount.Should().Be(1_200_000m);
            response.NightShiftExtraAmount.Should().Be(450_000m);
            response.FridayWorkAllowance.Should().Be(375_000m);
            response.CalculatedTaxAmount.Should().Be(4_500_000m);
            response.GrossAmount.Should().Be(25_000_000m);
            response.InsuranceAmount.Should().Be(1_750_000m);
            response.TotalDeductionsAmount.Should().Be(6_250_000m);
            response.NetPayableAmount.Should().Be(18_750_000m);
        }
    }

    [Fact]
    public void Create_WithGeneratedId_ShouldReturnSuccess()
    {
        var employeeId = Guid.NewGuid();

        var result = PayrollRecord.Create(
            employeeId,
            PeriodStart,
            PeriodEnd,
            false,
            20m,
            12m,
            _builder.WithPeriod(PeriodStart, PeriodEnd).BuildPayrollWorkInput(),
            _builder.BuildAmountsDto(),
            _builder.BuildCalculatedAmountsDto());

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.EmployeeId.Should().Be(employeeId);
            response.PeriodStart.Should().Be(PeriodStart);
            response.PeriodEnd.Should().Be(PeriodEnd);
        }
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه فیش پرداختی");
    }

    [Fact]
    public void Create_WithEmptyEmployeeId_ShouldFail()
    {
        var result = _builder.WithEmployeeId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه کارمند");
    }

    [Fact]
    public void Create_WithNullPayrollAmounts_ShouldFail()
    {
        var result = PayrollRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PeriodStart,
            PeriodEnd,
            false,
            20m,
            12m,
            _builder.BuildPayrollWorkInput(),
            null,
            _builder.BuildCalculatedAmountsDto());

        result.ShouldBeFailure("مبالغ فیش پرداختی نمیتواند خالی باشد.");
    }

    [Fact]
    public void Create_WithNullCalculatedAmounts_ShouldFail()
    {
        var result = PayrollRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PeriodStart,
            PeriodEnd,
            false,
            20m,
            12m,
            _builder.BuildPayrollWorkInput(),
            _builder.BuildAmountsDto(),
            null);

        result.ShouldBeFailure("مبالغ محاسبه شده فیش پرداختی نمیتواند خالی باشد.");
    }

    [Fact]
    public void Create_WithPeriodEndBeforePeriodStart_ShouldFail()
    {
        var result = _builder
            .WithPeriod(PeriodEnd, PeriodStart)
            .CreateResult();

        result.ShouldBeFailure("تاریخ پایان دوره نباید قبل از تاریخ شروع دوره باشد.");
    }

    [Fact]
    public void Create_WithSingleDayPeriod_ShouldReturnSuccess()
    {
        var result = _builder
            .WithPeriod(PeriodStart, PeriodStart)
            .WithWorkedDaysCount(1m)
            .WithLeaveHours(0m)
            .WithMissionDaysCount(0m)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.PeriodStart.Should().Be(PeriodStart);
            response.PeriodEnd.Should().Be(PeriodStart);
        }
    }

    [Fact]
    public void Create_WithPeriodOfMaxAllowedLength_ShouldReturnSuccess()
    {
        var result = _builder
            .WithPeriod(PeriodStart, PeriodStart.AddDays(PayrollRecord.MaxPeriodLengthInDays - 1))
            .CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithPeriodLongerThanMaxAllowed_ShouldFail()
    {
        var result = _builder
            .WithPeriod(PeriodStart, PeriodStart.AddDays(PayrollRecord.MaxPeriodLengthInDays))
            .CreateResult();

        result.ShouldBeFailure("بازه دوره فیش پرداختی نباید بیشتر از 31 روز باشد.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(32)]
    [InlineData(60)]
    public void Create_WithWorkedDaysCountOutOfRange_ShouldFail(int daysCount)
    {
        var result = _builder.WithWorkedDaysCount(daysCount).CreateResult();

        result.ShouldBeFailure("تعداد روزهای کارکرد باید بین 0 تا 31 روز باشد.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15.5)]
    [InlineData(31)]
    public void Create_WithValidWorkedDaysCount_ShouldReturnSuccess(double daysCount)
    {
        var result = _builder.WithWorkedDaysCount((decimal)daysCount).CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNegativeOvertimeHours_ShouldFail()
    {
        var result = _builder.WithOvertimeHours(-0.5m).CreateResult();

        result.ShouldBeFailure("ساعات اضافه‌کاری نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithOvertimeHoursAboveMaxMonthlyOvertimeHours_ShouldFail()
    {
        var result = _builder
            .WithOvertimeHours(20.5m)
            .WithMaxMonthlyOvertimeHours(20m)
            .CreateResult();

        result.ShouldBeFailure("ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.");
    }

    [Fact]
    public void Create_WithOvertimeHoursEqualToMaxMonthlyOvertimeHours_ShouldReturnSuccess()
    {
        var result = _builder
            .WithOvertimeHours(20m)
            .WithMaxMonthlyOvertimeHours(20m)
            .CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNullMaxMonthlyOvertimeHours_ShouldFail()
    {
        var result = _builder.WithMaxMonthlyOvertimeHours(null).CreateResult();

        result.ShouldBeFailure("حداکثر ساعات اضافه‌کاری ماهانه نمیتواند خالی باشد.");
    }

    [Fact]
    public void Create_WithNegativeNightShiftHours_ShouldFail()
    {
        var result = _builder.WithNightShiftHours(-1m).CreateResult();

        result.ShouldBeFailure("ساعات شیفت شب نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithFractionalNightShiftHours_ShouldReturnSuccess()
    {
        var result = _builder.WithNightShiftHours(2.25m).CreateResult();

        result.ShouldBeSuccess().NightShiftHours.Should().Be(2.25m);
    }

    [Fact]
    public void Create_WithNegativeFridayWorkHours_ShouldFail()
    {
        var result = _builder.WithFridayWorkHours(-2m).CreateResult();

        result.ShouldBeFailure("ساعات کار جمعه نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithFridayWorkHoursAboveMaxFridayHours_ShouldFail()
    {
        var result = _builder
            .WithFridayWorkHours(12.5m)
            .WithMaxFridayHours(12m)
            .CreateResult();

        result.ShouldBeFailure("ساعات کار جمعه نباید بیشتر از حداکثر ساعات کار جمعه باشد.");
    }

    [Fact]
    public void Create_WithFridayWorkHoursEqualToMaxFridayHours_ShouldReturnSuccess()
    {
        var result = _builder
            .WithFridayWorkHours(12m)
            .WithMaxFridayHours(12m)
            .CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNullMaxFridayHours_ShouldFail()
    {
        var result = _builder.WithMaxFridayHours(null).CreateResult();

        result.ShouldBeFailure("حداکثر ساعات کار جمعه نمیتواند خالی باشد.");
    }

    [Fact]
    public void Create_WithNegativeLeaveHours_ShouldFail()
    {
        var result = _builder.WithLeaveHours(-0.5m).CreateResult();

        result.ShouldBeFailure("ساعات مرخصی نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithLargeLeaveHours_ShouldReturnSuccess()
    {
        var result = _builder.WithLeaveHours(31.5m).CreateResult();

        result.ShouldBeSuccess().LeaveHours.Should().Be(31.5m);
    }

    [Fact]
    public void Create_WithNegativeAbsenceDaysCount_ShouldFail()
    {
        var result = _builder.WithAbsenceDaysCount(-1m).CreateResult();

        result.ShouldBeFailure("تعداد روزهای غیبت باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Create_WithAbsenceDaysCountAboveMax_ShouldFail()
    {
        var result = _builder.WithAbsenceDaysCount(32m).CreateResult();

        result.ShouldBeFailure("تعداد روزهای غیبت باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Create_WithNegativeMissionDaysCount_ShouldFail()
    {
        var result = _builder.WithMissionDaysCount(-3m).CreateResult();

        result.ShouldBeFailure("تعداد روزهای مأموریت باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Create_WithMissionDaysCountAboveMax_ShouldFail()
    {
        var result = _builder.WithMissionDaysCount(31.5m).CreateResult();

        result.ShouldBeFailure("تعداد روزهای مأموریت باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Create_WithNegativeMissionHours_ShouldFail()
    {
        var result = _builder.WithMissionHours(-4m).CreateResult();

        result.ShouldBeFailure("ساعات مأموریت نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithFractionalMissionHours_ShouldReturnSuccess()
    {
        var result = _builder.WithMissionHours(8.5m).CreateResult();

        result.ShouldBeSuccess().MissionHours.Should().Be(8.5m);
    }

    [Fact]
    public void Create_WithNegativeHolidayWorkHours_ShouldFail()
    {
        var result = _builder.WithHolidayWorkHours(-2m).CreateResult();

        result.ShouldBeFailure("ساعات تعطیل‌کاری نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithFractionalHolidayWorkHours_ShouldReturnSuccess()
    {
        var result = _builder.WithHolidayWorkHours(6.75m).CreateResult();

        result.ShouldBeSuccess().HolidayWorkHours.Should().Be(6.75m);
    }

    [Fact]
    public void Create_WithNegativeMissionAmountOverride_ShouldFail()
    {
        var result = _builder.WithMissionAmountOverride(-100_000m).CreateResult();

        result.ShouldBeFailure("مبلغ مأموریت نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithMissionAmountOverride_ShouldReturnSuccess()
    {
        var result = _builder.WithMissionAmountOverride(750_000m).CreateResult();

        result.ShouldBeSuccess().MissionAmountOverride.Should().Be(750_000m);
    }

    [Theory]
    [InlineData(27)]
    [InlineData(32)]
    [InlineData(40)]
    public void Create_WithStandardWorkingDaysCountOutOfRange_ShouldFail(int standardWorkingDaysCount)
    {
        var result = _builder
            .WithStandardWorkingDaysCount(standardWorkingDaysCount)
            .CreateResult();

        result.ShouldBeFailure("تعداد روزهای کارکرد استاندارد باید بین 28 تا 31 روز باشد.");
    }

    [Theory]
    [InlineData(28)]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    public void Create_WithValidStandardWorkingDaysCount_ShouldReturnSuccess(int standardWorkingDaysCount)
    {
        var result = _builder
            .WithStandardWorkingDaysCount(standardWorkingDaysCount)
            .CreateResult();

        result.ShouldBeSuccess().StandardWorkingDaysCount.Should().Be(standardWorkingDaysCount);
    }

    [Fact]
    public void Create_WithWorkedDaysCountExceedingStandardWorkingDaysCount_ShouldFail()
    {
        var result = _builder
            .WithWorkedDaysCount(30m)
            .WithStandardWorkingDaysCount(29)
            .CreateResult();

        result.ShouldBeFailure("تعداد روزهای کارکرد نمیتواند بیشتر از روزهای کارکرد استاندارد باشد.");
    }

    [Fact]
    public void Create_WithWorkedDaysCountEqualToStandardWorkingDaysCount_ShouldReturnSuccess()
    {
        var result = _builder
            .WithWorkedDaysCount(29m)
            .WithStandardWorkingDaysCount(29)
            .CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithAnnualBonusTypeOutsideEsfandPeriod_ShouldFail()
    {
        var result = _builder
            .WithIsEsfandPeriod(false)
            .WithAnnualBonusType(AnnualBonusType.Minimum)
            .CreateResult();

        result.ShouldBeFailure("عیدی سالانه فقط در ماه اسفند قابل ثبت است.");
    }

    [Fact]
    public void Create_WithEsfandPeriodWithoutAnnualBonusType_ShouldFail()
    {
        var result = _builder
            .WithIsEsfandPeriod(true)
            .WithAnnualBonusType(null)
            .CreateResult();

        result.ShouldBeFailure("نوع عیدی سالانه نمیتواند خالی باشد.");
    }

    [Fact]
    public void Create_WithEsfandPeriodAndAnnualBonusType_ShouldReturnSuccess()
    {
        var result = _builder
            .WithIsEsfandPeriod(true)
            .WithAnnualBonusType(AnnualBonusType.Minimum)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.IsEsfandPeriod.Should().BeTrue();
            response.AnnualBonusType.Should().Be(AnnualBonusType.Minimum);
        }
    }

    [Fact]
    public void Create_WithNegativePerformanceBonusAmount_ShouldFail()
    {
        var result = _builder.WithPerformanceBonusAmount(-1_000_000m).CreateResult();

        result.ShouldBeFailure("مبلغ کارانه نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithPerformanceBonusAmount_ShouldReturnSuccess()
    {
        var result = _builder.WithPerformanceBonusAmount(1_000_000m).CreateResult();

        result.ShouldBeSuccess().PerformanceBonusAmount.Should().Be(1_000_000m);
    }

    [Fact]
    public void Create_WithNegativeCashBenefitsAmount_ShouldFail()
    {
        var result = _builder.WithCashBenefitsAmount(-200_000m).CreateResult();

        result.ShouldBeFailure("مبلغ مزایای نقدی نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithCashBenefitsAmount_ShouldReturnSuccess()
    {
        var result = _builder.WithCashBenefitsAmount(200_000m).CreateResult();

        result.ShouldBeSuccess().CashBenefitsAmount.Should().Be(200_000m);
    }

    [Fact]
    public void Create_WithZeroDayCounts_ShouldReturnSuccess()
    {
        var result = _builder
            .WithWorkedDaysCount(0m)
            .WithOvertimeHours(0m)
            .WithNightShiftHours(0m)
            .WithFridayWorkHours(0m)
            .WithLeaveHours(0m)
            .WithAbsenceDaysCount(0m)
            .WithMissionDaysCount(0m)
            .WithMissionHours(0m)
            .WithHolidayWorkHours(0m)
            .CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNegativeCalculatedTaxAmount_ShouldFail()
    {
        var result = _builder.WithCalculatedTaxAmount(-1m).CreateResult();

        result.ShouldBeFailure("مالیات محاسبه شده نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithZeroTaxForNonTaxSubjectEmployee_ShouldReturnSuccess()
    {
        var result = _builder
            .WithEmployeeIsTaxSubject(false)
            .WithCalculatedTaxAmount(0m)
            .CreateResult();

        result.ShouldBeSuccess().CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public void Create_WithZeroTaxForTaxSubjectEmployee_ShouldFail()
    {
        var result = _builder
            .WithEmployeeIsTaxSubject(true)
            .WithCalculatedTaxAmount(0m)
            .CreateResult();

        result.ShouldBeFailure("برای کارمند مشمول مالیات، مالیات محاسبه شده نمیتواند صفر باشد.");
    }

    [Fact]
    public void Create_WithPositiveTaxForTaxSubjectEmployee_ShouldReturnSuccess()
    {
        var result = _builder
            .WithEmployeeIsTaxSubject(true)
            .WithCalculatedTaxAmount(1_000m)
            .CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNegativeOvertimeAmount_ShouldFail()
    {
        var result = _builder.WithOvertimeAmount(-1m).CreateResult();

        result.ShouldBeFailure("مبلغ اضافه‌کاری نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithNegativeNightShiftExtraAmount_ShouldFail()
    {
        var result = _builder.WithNightShiftExtraAmount(-0.5m).CreateResult();

        result.ShouldBeFailure("فوق‌العاده شیفت شب نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithNegativeFridayWorkAllowance_ShouldFail()
    {
        var result = _builder.WithFridayWorkAllowance(-250_000m).CreateResult();

        result.ShouldBeFailure("حق کار جمعه نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithNegativeGrossAmount_ShouldFail()
    {
        var result = _builder.WithGrossAmount(-1m).CreateResult();

        result.ShouldBeFailure("جمع حقوق و مزایا نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithNegativeInsuranceAmount_ShouldFail()
    {
        var result = _builder.WithInsuranceAmount(-0.5m).CreateResult();

        result.ShouldBeFailure("بیمه ۷٪ نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithNegativeTotalDeductionsAmount_ShouldFail()
    {
        var result = _builder.WithTotalDeductionsAmount(-1m).CreateResult();

        result.ShouldBeFailure("مالیات و کسورات نمیتواند منفی باشد.");
    }

    [Fact]
    public void Create_WithZeroAmounts_ShouldReturnSuccess()
    {
        var result = _builder
            .WithOvertimeAmount(0m)
            .WithNightShiftExtraAmount(0m)
            .WithFridayWorkAllowance(0m)
            .WithCalculatedTaxAmount(0m)
            .WithGrossAmount(0m)
            .WithInsuranceAmount(0m)
            .WithTotalDeductionsAmount(0m)
            .WithNetPayableAmount(0m)
            .CreateResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithFractionalAmounts_ShouldReturnSuccess()
    {
        var result = _builder
            .WithOvertimeAmount(125_000.5m)
            .WithNetPayableAmount(9_999_999.75m)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.OvertimeAmount.Should().Be(125_000.5m);
            response.NetPayableAmount.Should().Be(9_999_999.75m);
        }
    }

    [Fact]
    public void Create_WithNegativeNetPayableAmount_ShouldFail()
    {
        var result = _builder
            .WithNetPayableAmount(-125_000m)
            .CreateResult();

        result.ShouldBeFailure("حقوق نهایی نمیتواند منفی باشد.");
    }

    [Theory]
    [InlineData("BaseSalaryAmount")]
    [InlineData("AttractionAllowanceAmount")]
    [InlineData("SupervisionAllowanceAmount")]
    [InlineData("NightShiftExtraAmount")]
    [InlineData("HolidayWorkAmount")]
    [InlineData("ChildAllowanceAmount")]
    [InlineData("HousingAllowanceAmount")]
    [InlineData("FoodAllowanceAmount")]
    [InlineData("MarriageAllowanceAmount")]
    [InlineData("OvertimeAmount")]
    [InlineData("ShiftWorkAmount")]
    [InlineData("DailyMissionAmount")]
    [InlineData("FridayWorkAllowance")]
    [InlineData("EndOfServiceAmount")]
    [InlineData("AnnualBonusAmount")]
    [InlineData("CommutingAllowanceAmount")]
    public void Create_WithNegativeCalculatedAmount_ShouldFail(string fieldName)
    {
        var calculatedAmounts = WithNegativeCalculatedAmount(_builder.BuildCalculatedAmountsDto(), fieldName);

        var result = PayrollRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PeriodStart,
            PeriodEnd,
            false,
            20m,
            12m,
            _builder.BuildPayrollWorkInput(),
            _builder.BuildAmountsDto(),
            calculatedAmounts);

        result.ShouldBeFailure("نمیتواند منفی باشد");
    }

    [Fact]
    public void Create_WithCalculatedAmounts_ShouldStoreTheStoredAmountFields()
    {
        var result = _builder
            .WithBaseSalaryAmount(12_000_000m)
            .WithAttractionAllowanceAmount(500_000m)
            .WithSupervisionAllowanceAmount(400_000m)
            .WithHolidayWorkAmount(600_000m)
            .WithChildAllowanceAmount(700_000m)
            .WithHousingAllowanceAmount(800_000m)
            .WithFoodAllowanceAmount(900_000m)
            .WithMarriageAllowanceAmount(1_000_000m)
            .WithShiftWorkAmount(1_100_000m)
            .WithDailyMissionAmount(1_200_000m)
            .WithEndOfServiceAmount(1_300_000m)
            .WithCalculatedAnnualBonusAmount(1_400_000m)
            .WithCommutingAllowanceAmount(1_500_000m)
            .WithOvertimeAmount(1_600_000m)
            .WithNightShiftExtraAmount(1_700_000m)
            .WithFridayWorkAllowance(1_800_000m)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.OvertimeAmount.Should().Be(1_600_000m);
            response.NightShiftExtraAmount.Should().Be(1_700_000m);
            response.FridayWorkAllowance.Should().Be(1_800_000m);
        }
    }

    [Fact]
    public void Create_WithTotals_ShouldStoreGrossInsuranceAndTotalDeductions()
    {
        var result = _builder
            .WithGrossAmount(22_000_000m)
            .WithInsuranceAmount(1_540_000m)
            .WithTotalDeductionsAmount(3_040_000m)
            .WithNetPayableAmount(18_960_000m)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.GrossAmount.Should().Be(22_000_000m);
            response.InsuranceAmount.Should().Be(1_540_000m);
            response.TotalDeductionsAmount.Should().Be(3_040_000m);
            response.NetPayableAmount.Should().Be(18_960_000m);
        }
    }

    private static PayrollCalculatedAmountsDto WithNegativeCalculatedAmount(
        PayrollCalculatedAmountsDto calculatedAmounts,
        string fieldName) =>
        fieldName switch
        {
            "BaseSalaryAmount" => calculatedAmounts with { BaseSalaryAmount = -1m },
            "AttractionAllowanceAmount" => calculatedAmounts with { AttractionAllowanceAmount = -1m },
            "SupervisionAllowanceAmount" => calculatedAmounts with { SupervisionAllowanceAmount = -1m },
            "NightShiftExtraAmount" => calculatedAmounts with { NightShiftExtraAmount = -1m },
            "HolidayWorkAmount" => calculatedAmounts with { HolidayWorkAmount = -1m },
            "ChildAllowanceAmount" => calculatedAmounts with { ChildAllowanceAmount = -1m },
            "HousingAllowanceAmount" => calculatedAmounts with { HousingAllowanceAmount = -1m },
            "FoodAllowanceAmount" => calculatedAmounts with { FoodAllowanceAmount = -1m },
            "MarriageAllowanceAmount" => calculatedAmounts with { MarriageAllowanceAmount = -1m },
            "OvertimeAmount" => calculatedAmounts with { OvertimeAmount = -1m },
            "ShiftWorkAmount" => calculatedAmounts with { ShiftWorkAmount = -1m },
            "DailyMissionAmount" => calculatedAmounts with { DailyMissionAmount = -1m },
            "FridayWorkAllowance" => calculatedAmounts with { FridayWorkAllowance = -1m },
            "EndOfServiceAmount" => calculatedAmounts with { EndOfServiceAmount = -1m },
            "AnnualBonusAmount" => calculatedAmounts with { AnnualBonusAmount = -1m },
            _ => calculatedAmounts with { CommutingAllowanceAmount = -1m }
        };
}