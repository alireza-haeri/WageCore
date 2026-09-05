namespace Core.Tests.Domain.PayrollRecords;

public class UpdatePayrollRecordTests
{
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);
    private static readonly DateOnly NewPeriodStart = new(2025, 2, 1);
    private static readonly DateOnly NewPeriodEnd = new(2025, 2, 28);

    private readonly PayrollRecordBuilder _builder = new();

    private PayrollRecord CreateRecord() =>
        _builder
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

    private static PayrollWorkInput BuildWorkInput() =>
        new(
            20,
            3m,
            2m,
            1m,
            4m,
            2m,
            0m,
            0m,
            1,
            null,
            31,
            false,
            null,
            null,
            null);

    private static PayrollRecordAmountsDto BuildAmounts(bool withZeroTax = false) =>
        new(
            withZeroTax ? 0m : 2_000_000m,
            14_000_000m,
            980_000m,
            2_980_000m,
            12_500_000m);

    private static PayrollCalculatedAmountsDto BuildCalculatedAmounts() =>
        new(
            10_000_000m,
            0m,
            0m,
            300_000m,
            0m,
            0m,
            0m,
            0m,
            0m,
            800_000m,
            0m,
            0m,
            250_000m,
            0m,
            0m,
            0m,
            null,
            null);

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var record = CreateRecord();

        var result = record.Update(
            NewPeriodStart,
            NewPeriodEnd,
            20m,
            12m,
            3m,
            8m,
            BuildWorkInput(),
            BuildAmounts(),
            BuildCalculatedAmounts());

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            record.PeriodStart.Should().Be(NewPeriodStart);
            record.PeriodEnd.Should().Be(NewPeriodEnd);
            record.WorkedDaysCount.Should().Be(20);
            record.OvertimeHours.Should().Be(3m);
            record.NightShiftHours.Should().Be(2m);
            record.FridayWorkHours.Should().Be(1m);
            record.LeaveHours.Should().Be(4m);
            record.HolidaysCount.Should().Be(1);
            record.MissionDaysCount.Should().Be(2m);
            record.MissionHours.Should().Be(0m);
            record.HolidayWorkHours.Should().Be(0m);
            record.MissionAmountOverride.Should().BeNull();
            record.StandardWorkingDaysCount.Should().Be(31);
            record.IsEsfandPeriod.Should().BeFalse();
            record.AnnualBonusType.Should().BeNull();
            record.PerformanceBonusAmount.Should().BeNull();
            record.CashBenefitsAmount.Should().BeNull();
            record.OvertimeAmount.Should().Be(800_000m);
            record.NightShiftExtraAmount.Should().Be(300_000m);
            record.FridayWorkAllowance.Should().Be(250_000m);
            record.CalculatedTaxAmount.Should().Be(2_000_000m);
            record.GrossAmount.Should().Be(14_000_000m);
            record.InsuranceAmount.Should().Be(980_000m);
            record.TotalDeductionsAmount.Should().Be(2_980_000m);
            record.NetPayableAmount.Should().Be(12_500_000m);
        }
    }

    [Fact]
    public void Update_ShouldNotChangeIdAndEmployeeId()
    {
        var record = CreateRecord();
        var id = record.Id;
        var employeeId = record.EmployeeId;

        var result = record.Update(
            NewPeriodStart,
            NewPeriodEnd,
            20m,
            12m,
            3m,
            8m,
            BuildWorkInput(),
            BuildAmounts(),
            BuildCalculatedAmounts());

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            record.Id.Should().Be(id);
            record.EmployeeId.Should().Be(employeeId);
        }
    }

    [Fact]
    public void Update_ShouldNotChangeStatus()
    {
        var record = CreateRecord();

        var result = record.Update(
            NewPeriodStart,
            NewPeriodEnd,
            20m,
            12m,
            3m,
            8m,
            BuildWorkInput(),
            BuildAmounts(),
            BuildCalculatedAmounts());

        result.ShouldBeSuccess();
        record.Status.Should().Be(PayrollRecordStatus.Draft);
    }

    [Fact]
    public void Update_WhenRecordIsPaid_ShouldFail()
    {
        var record = CreateRecord();
        record.MarkAsPaid().ShouldBeSuccess();

        var result = record.Update(
            NewPeriodStart,
            NewPeriodEnd,
            20m,
            12m,
            3m,
            8m,
            BuildWorkInput(),
            BuildAmounts(),
            BuildCalculatedAmounts());

        result.ShouldBeFailure("فیش پرداختی پرداخت شده قابل ویرایش نیست.");
        using (new AssertionScope())
        {
            record.PeriodStart.Should().Be(PeriodStart);
            record.WorkedDaysCount.Should().Be(24);
        }
    }

    [Fact]
    public void Update_WithPeriodEndBeforePeriodStart_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(
            NewPeriodEnd,
            NewPeriodStart,
            20m,
            12m,
            3m,
            8m,
            BuildWorkInput(),
            BuildAmounts(),
            BuildCalculatedAmounts());

        result.ShouldBeFailure("تاریخ پایان دوره نباید قبل از تاریخ شروع دوره باشد.");
    }

    [Fact]
    public void Update_WithPeriodLongerThanMaxAllowed_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(
            NewPeriodStart,
            NewPeriodStart.AddDays(PayrollRecord.MaxPeriodLengthInDays),
            20m,
            12m,
            3m,
            8m,
            BuildWorkInput(),
            BuildAmounts(),
            BuildCalculatedAmounts());

        result.ShouldBeFailure("بازه دوره فیش پرداختی نباید بیشتر از 31 روز باشد.");
    }

    [Fact]
    public void Update_WithNullPayrollAmounts_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, BuildWorkInput(), null, BuildCalculatedAmounts());

        result.ShouldBeFailure("مبالغ فیش پرداختی نمیتواند خالی باشد.");
    }

    [Fact]
    public void Update_WithNullCalculatedAmounts_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, BuildWorkInput(), BuildAmounts(), null);

        result.ShouldBeFailure("مبالغ محاسبه شده فیش پرداختی نمیتواند خالی باشد.");
    }

    [Fact]
    public void Update_WithNegativeHours_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { NightShiftHours = -1m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("ساعات شیفت شب نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WithNegativeMissionHours_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { MissionHours = -1m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("ساعات مأموریت نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WithOvertimeHoursAboveMaxMonthlyOvertimeHours_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { OvertimeHours = 21m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.");
    }

    [Fact]
    public void Update_WithFridayWorkHoursAboveMaxFridayHours_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { FridayWorkHours = 13m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("ساعات کار جمعه نباید بیشتر از حداکثر ساعات کار جمعه باشد.");
    }

    [Fact]
    public void Update_WithNightShiftHoursAboveMaxNightShiftHours_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { NightShiftHours = 4m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("ساعات شب‌کاری نباید بیشتر از حداکثر ساعات شب‌کاری ماهانه باشد.");
    }

    [Fact]
    public void Update_WithDaysCountOutOfRange_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { HolidaysCount = 32 };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("تعداد روزهای تعطیل باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Update_WithWorkedDaysCountExceedingStandardWorkingDaysCount_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { WorkedDaysCount = 30, StandardWorkingDaysCount = 29 };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("تعداد روزهای کارکرد نمیتواند بیشتر از روزهای کارکرد استاندارد باشد.");
    }

    [Fact]
    public void Update_WithAnnualBonusTypeOutsideEsfandPeriod_ShouldFail()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { AnnualBonusType = AnnualBonusType.Minimum };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("عیدی سالانه فقط در ماه اسفند قابل ثبت است.");
    }

    [Fact]
    public void Update_WithEsfandPeriodAndAnnualBonusType_ShouldReturnSuccess()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Minimum
        };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeSuccess();
        record.AnnualBonusType.Should().Be(AnnualBonusType.Minimum);
    }

    [Fact]
    public void Update_WithZeroCalculatedTaxAmount_ShouldReturnSuccess()
    {
        var record = CreateRecord();
        var amounts = BuildAmounts(withZeroTax: true);

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, BuildWorkInput(), amounts, BuildCalculatedAmounts());

        result.ShouldBeSuccess();
        record.CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public void Update_WithNegativeAmount_ShouldFail()
    {
        var record = CreateRecord();
        var calculatedAmounts = BuildCalculatedAmounts() with { OvertimeAmount = -1m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, BuildWorkInput(), BuildAmounts(), calculatedAmounts);

        result.ShouldBeFailure("مبلغ اضافه‌کاری نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WithNegativeGrossAmount_ShouldFail()
    {
        var record = CreateRecord();
        var amounts = BuildAmounts() with { GrossAmount = -1m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, BuildWorkInput(), amounts, BuildCalculatedAmounts());

        result.ShouldBeFailure("جمع حقوق و مزایا نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WithNegativeNetPayableAmount_ShouldFail()
    {
        var record = CreateRecord();
        var amounts = BuildAmounts() with { NetPayableAmount = -125_000m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, BuildWorkInput(), amounts, BuildCalculatedAmounts());

        result.ShouldBeFailure("حقوق نهایی نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WhenValidationFails_ShouldKeepPreviousValues()
    {
        var record = CreateRecord();
        var workInput = BuildWorkInput() with { MissionDaysCount = 99m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, 20m, 12m, 3m, 8m, workInput, BuildAmounts(), BuildCalculatedAmounts());

        result.ShouldBeFailure("تعداد روزهای مأموریت باید بین 0 تا 31 روز باشد.");
        using (new AssertionScope())
        {
            record.PeriodStart.Should().Be(PeriodStart);
            record.PeriodEnd.Should().Be(PeriodEnd);
            record.WorkedDaysCount.Should().Be(24);
            record.LeaveHours.Should().Be(2m);
            record.OvertimeAmount.Should().Be(800_000m);
            record.NetPayableAmount.Should().Be(15_000_000m);
            record.CalculatedTaxAmount.Should().Be(1_500_000m);
            record.GrossAmount.Should().Be(17_900_000m);
            record.InsuranceAmount.Should().Be(1_400_000m);
            record.TotalDeductionsAmount.Should().Be(2_900_000m);
        }
    }
}