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
            response.LeaveDaysCount.Should().Be(2m);
            response.AbsenceDaysCount.Should().Be(0m);
            response.MissionDaysCount.Should().Be(1m);
            response.OvertimeAmount.Should().Be(800_000m);
            response.NightShiftExtraAmount.Should().Be(300_000m);
            response.FridayWorkAllowance.Should().Be(250_000m);
            response.CalculatedTaxAmount.Should().Be(1_500_000m);
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
            .WithLeaveDaysCount(2.5m)
            .WithAbsenceDaysCount(1.5m)
            .WithMissionDaysCount(3.75m)
            .WithOvertimeAmount(1_200_000m)
            .WithNightShiftExtraAmount(450_000m)
            .WithFridayWorkAllowance(375_000m)
            .WithCalculatedTaxAmount(4_500_000m)
            .WithNetPayableAmount(18_725_000m)
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
            response.LeaveDaysCount.Should().Be(2.5m);
            response.AbsenceDaysCount.Should().Be(1.5m);
            response.MissionDaysCount.Should().Be(3.75m);
            response.OvertimeAmount.Should().Be(1_200_000m);
            response.NightShiftExtraAmount.Should().Be(450_000m);
            response.FridayWorkAllowance.Should().Be(375_000m);
            response.CalculatedTaxAmount.Should().Be(4_500_000m);
            response.NetPayableAmount.Should().Be(18_725_000m);
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
            _builder.WithPeriod(PeriodStart, PeriodEnd).BuildDto(),
            _builder.BuildAmountsDto());

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
    public void Create_WithNullPayrollRecord_ShouldFail()
    {
        var result = PayrollRecord.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PeriodStart,
            PeriodEnd,
            false,
            20m,
            12m,
            null,
            _builder.BuildAmountsDto());

        result.ShouldBeFailure("اطلاعات فیش پرداختی");
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
            _builder.BuildDto(),
            null);

        result.ShouldBeFailure("مبالغ فیش پرداختی نمیتواند خالی باشد.");
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
            .WithLeaveDaysCount(0m)
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

    [Fact]
    public void Create_WithNullWorkedDaysCount_ShouldFail()
    {
        var result = _builder.WithWorkedDaysCount(null).CreateResult();

        result.ShouldBeFailure("تعداد روزهای کارکرد");
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
    public void Create_WithNullOvertimeHours_ShouldFail()
    {
        var result = _builder.WithOvertimeHours(null).CreateResult();

        result.ShouldBeFailure("ساعات اضافه‌کاری");
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
    public void Create_WithNullNightShiftHours_ShouldFail()
    {
        var result = _builder.WithNightShiftHours(null).CreateResult();

        result.ShouldBeFailure("ساعات شیفت شب");
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
    public void Create_WithNullFridayWorkHours_ShouldFail()
    {
        var result = _builder.WithFridayWorkHours(null).CreateResult();

        result.ShouldBeFailure("ساعات کار جمعه");
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
    public void Create_WithNullLeaveDaysCount_ShouldFail()
    {
        var result = _builder.WithLeaveDaysCount(null).CreateResult();

        result.ShouldBeFailure("تعداد روزهای مرخصی");
    }

    [Fact]
    public void Create_WithNegativeLeaveDaysCount_ShouldFail()
    {
        var result = _builder.WithLeaveDaysCount(-0.5m).CreateResult();

        result.ShouldBeFailure("تعداد روزهای مرخصی باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Create_WithLeaveDaysCountAboveMax_ShouldFail()
    {
        var result = _builder.WithLeaveDaysCount(31.5m).CreateResult();

        result.ShouldBeFailure("تعداد روزهای مرخصی باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Create_WithNullAbsenceDaysCount_ShouldFail()
    {
        var result = _builder.WithAbsenceDaysCount(null).CreateResult();

        result.ShouldBeFailure("تعداد روزهای غیبت");
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
    public void Create_WithNullMissionDaysCount_ShouldFail()
    {
        var result = _builder.WithMissionDaysCount(null).CreateResult();

        result.ShouldBeFailure("تعداد روزهای مأموریت");
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
    public void Create_WithZeroDayCounts_ShouldReturnSuccess()
    {
        var result = _builder
            .WithWorkedDaysCount(0m)
            .WithOvertimeHours(0m)
            .WithNightShiftHours(0m)
            .WithFridayWorkHours(0m)
            .WithLeaveDaysCount(0m)
            .WithAbsenceDaysCount(0m)
            .WithMissionDaysCount(0m)
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
    public void Create_WithZeroAmounts_ShouldReturnSuccess()
    {
        var result = _builder
            .WithOvertimeAmount(0m)
            .WithNightShiftExtraAmount(0m)
            .WithFridayWorkAllowance(0m)
            .WithCalculatedTaxAmount(0m)
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
    public void Create_WithNegativeNetPayableAmount_ShouldReturnSuccess()
    {
        var result = _builder
            .WithNetPayableAmount(-125_000m)
            .CreateResult();

        result.ShouldBeSuccess().NetPayableAmount.Should().Be(-125_000m);
    }
}
