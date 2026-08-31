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

    private static PayrollWorkInputDto BuildDto() =>
        new(
            20m,
            3m,
            2m,
            1m,
            4m,
            1m,
            2m);

    private static PayrollRecordAmountsDto BuildAmounts(bool withZeroTax = false) =>
        new(
            800_000m,
            300_000m,
            250_000m,
            withZeroTax ? 0m : 2_000_000m,
            12_500_000m);

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var record = CreateRecord();

        var result = record.Update(
            NewPeriodStart,
            NewPeriodEnd,
            false,
            20m,
            12m,
            BuildDto(),
            BuildAmounts());

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            record.PeriodStart.Should().Be(NewPeriodStart);
            record.PeriodEnd.Should().Be(NewPeriodEnd);
            record.WorkedDaysCount.Should().Be(20m);
            record.OvertimeHours.Should().Be(3m);
            record.NightShiftHours.Should().Be(2m);
            record.FridayWorkHours.Should().Be(1m);
            record.LeaveDaysCount.Should().Be(4m);
            record.AbsenceDaysCount.Should().Be(1m);
            record.MissionDaysCount.Should().Be(2m);
            record.OvertimeAmount.Should().Be(800_000m);
            record.NightShiftExtraAmount.Should().Be(300_000m);
            record.FridayWorkAllowance.Should().Be(250_000m);
            record.CalculatedTaxAmount.Should().Be(2_000_000m);
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
            false,
            20m,
            12m,
            BuildDto(),
            BuildAmounts());

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
            false,
            20m,
            12m,
            BuildDto(),
            BuildAmounts());

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
            false,
            20m,
            12m,
            BuildDto(),
            BuildAmounts());

        result.ShouldBeFailure("فیش پرداختی پرداخت شده قابل ویرایش نیست.");
        using (new AssertionScope())
        {
            record.PeriodStart.Should().Be(PeriodStart);
            record.WorkedDaysCount.Should().Be(24m);
        }
    }

    [Fact]
    public void Update_WithPeriodEndBeforePeriodStart_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(
            NewPeriodEnd,
            NewPeriodStart,
            false,
            20m,
            12m,
            BuildDto(),
            BuildAmounts());

        result.ShouldBeFailure("تاریخ پایان دوره نباید قبل از تاریخ شروع دوره باشد.");
    }

    [Fact]
    public void Update_WithPeriodLongerThanMaxAllowed_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(
            NewPeriodStart,
            NewPeriodStart.AddDays(PayrollRecord.MaxPeriodLengthInDays),
            false,
            20m,
            12m,
            BuildDto(),
            BuildAmounts());

        result.ShouldBeFailure("بازه دوره فیش پرداختی نباید بیشتر از 31 روز باشد.");
    }

    [Fact]
    public void Update_WithNullPayrollRecord_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, null, BuildAmounts());

        result.ShouldBeFailure("اطلاعات فیش پرداختی");
    }

    [Fact]
    public void Update_WithNullPayrollAmounts_ShouldFail()
    {
        var record = CreateRecord();

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, BuildDto(), null);

        result.ShouldBeFailure("مبالغ فیش پرداختی نمیتواند خالی باشد.");
    }

    [Fact]
    public void Update_WithNegativeHours_ShouldFail()
    {
        var record = CreateRecord();
        var dto = BuildDto() with { NightShiftHours = -1m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, dto, BuildAmounts());

        result.ShouldBeFailure("ساعات شیفت شب نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WithOvertimeHoursAboveMaxMonthlyOvertimeHours_ShouldFail()
    {
        var record = CreateRecord();
        var dto = BuildDto() with { OvertimeHours = 21m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, dto, BuildAmounts());

        result.ShouldBeFailure("ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.");
    }

    [Fact]
    public void Update_WithFridayWorkHoursAboveMaxFridayHours_ShouldFail()
    {
        var record = CreateRecord();
        var dto = BuildDto() with { FridayWorkHours = 13m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, dto, BuildAmounts());

        result.ShouldBeFailure("ساعات کار جمعه نباید بیشتر از حداکثر ساعات کار جمعه باشد.");
    }

    [Fact]
    public void Update_WithDaysCountOutOfRange_ShouldFail()
    {
        var record = CreateRecord();
        var dto = BuildDto() with { AbsenceDaysCount = 32m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, dto, BuildAmounts());

        result.ShouldBeFailure("تعداد روزهای غیبت باید بین 0 تا 31 روز باشد.");
    }

    [Fact]
    public void Update_WithZeroTaxForTaxSubjectEmployee_ShouldFail()
    {
        var record = CreateRecord();
        var amounts = BuildAmounts(withZeroTax: true);

        var result = record.Update(NewPeriodStart, NewPeriodEnd, true, 20m, 12m, BuildDto(), amounts);

        result.ShouldBeFailure("برای کارمند مشمول مالیات، مالیات محاسبه شده نمیتواند صفر باشد.");
    }

    [Fact]
    public void Update_WithZeroTaxForNonTaxSubjectEmployee_ShouldReturnSuccess()
    {
        var record = CreateRecord();
        var amounts = BuildAmounts(withZeroTax: true);

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, BuildDto(), amounts);

        result.ShouldBeSuccess();
        record.CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public void Update_WithNegativeAmount_ShouldFail()
    {
        var record = CreateRecord();
        var amounts = BuildAmounts() with { OvertimeAmount = -1m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, BuildDto(), amounts);

        result.ShouldBeFailure("مبلغ اضافه‌کاری نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WithNegativeNetPayableAmount_ShouldReturnSuccess()
    {
        var record = CreateRecord();
        var amounts = BuildAmounts() with { NetPayableAmount = -125_000m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, BuildDto(), amounts);

        result.ShouldBeSuccess();
        record.NetPayableAmount.Should().Be(-125_000m);
    }

    [Fact]
    public void Update_WhenValidationFails_ShouldKeepPreviousValues()
    {
        var record = CreateRecord();
        var dto = BuildDto() with { MissionDaysCount = 99m };

        var result = record.Update(NewPeriodStart, NewPeriodEnd, false, 20m, 12m, dto, BuildAmounts());

        result.ShouldBeFailure("تعداد روزهای مأموریت باید بین 0 تا 31 روز باشد.");
        using (new AssertionScope())
        {
            record.PeriodStart.Should().Be(PeriodStart);
            record.PeriodEnd.Should().Be(PeriodEnd);
            record.WorkedDaysCount.Should().Be(24m);
            record.LeaveDaysCount.Should().Be(2m);
            record.OvertimeAmount.Should().Be(800_000m);
            record.NetPayableAmount.Should().Be(15_000_000m);
            record.CalculatedTaxAmount.Should().Be(1_500_000m);
        }
    }
}
