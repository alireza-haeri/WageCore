namespace Core.Tests.Domain.PayrollRecords;

public class PayrollRecordStatusTests
{
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);

    private readonly PayrollRecordBuilder _builder = new();

    private PayrollRecord CreateDraftRecord() =>
        _builder
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

    [Fact]
    public void Create_ShouldStartAsDraft()
    {
        var record = CreateDraftRecord();

        using (new AssertionScope())
        {
            record.Status.Should().Be(PayrollRecordStatus.Draft);
            record.IsPaid.Should().BeFalse();
        }
    }

    [Fact]
    public void MarkAsPaid_WhenRecordIsDraft_ShouldReturnSuccess()
    {
        var record = CreateDraftRecord();

        var result = record.MarkAsPaid();

        using (new AssertionScope())
        {
            result.ShouldBeSuccess();
            record.Status.Should().Be(PayrollRecordStatus.Paid);
            record.IsPaid.Should().BeTrue();
        }
    }

    [Fact]
    public void MarkAsPaid_WhenRecordIsAlreadyPaid_ShouldFail()
    {
        var record = CreateDraftRecord();
        record.MarkAsPaid().ShouldBeSuccess();

        var result = record.MarkAsPaid();

        using (new AssertionScope())
        {
            result.ShouldBeFailure("فیش پرداختی قبلاً پرداخت شده است.");
            record.Status.Should().Be(PayrollRecordStatus.Paid);
        }
    }

    [Fact]
    public void MarkAsPaid_ShouldNotChangeOtherProperties()
    {
        var record = CreateDraftRecord();

        var result = record.MarkAsPaid();

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            record.WorkedDaysCount.Should().Be(24m);
            record.OvertimeHours.Should().Be(4m);
            record.CalculatedTaxAmount.Should().Be(1_500_000m);
        }
    }

    [Fact]
    public void EnsureCanDelete_WhenRecordIsDraft_ShouldReturnSuccess()
    {
        var record = CreateDraftRecord();

        record.EnsureCanDelete().ShouldBeSuccess();
    }

    [Fact]
    public void EnsureCanDelete_WhenRecordIsPaid_ShouldFail()
    {
        var record = CreateDraftRecord();
        record.MarkAsPaid().ShouldBeSuccess();

        record.EnsureCanDelete()
            .ShouldBeFailure("فیش پرداختی پرداخت شده قابل حذف نیست.");
    }

    [Fact]
    public void DraftRecord_ShouldBeUpdatableAndDeletable()
    {
        var record = CreateDraftRecord();
        var periodStart = new DateOnly(2025, 2, 1);
        var periodEnd = new DateOnly(2025, 2, 28);
        var workInput = new PayrollWorkInput(
            20m,
            3m,
            2m,
            1m,
            1m,
            1m,
            1m,
            0m,
            0m,
            null,
            31,
            false,
            null,
            null,
            null);
        var amounts = new PayrollRecordAmountsDto(
            1_000m,
            5_000_000m,
            350_000m,
            1_350_000m,
            5_000_000m);
        var calculatedAmounts = new PayrollCalculatedAmountsDto(
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

        using (new AssertionScope())
        {
            record.Update(periodStart, periodEnd, false, 20m, 12m, 3m, 8m, workInput, amounts, calculatedAmounts)
                .IsSuccess.Should().BeTrue();
            record.EnsureCanDelete().IsSuccess.Should().BeTrue();
        }
    }

    [Fact]
    public void PaidRecord_ShouldBeNeitherUpdatableNorDeletable()
    {
        var record = CreateDraftRecord();
        record.MarkAsPaid().ShouldBeSuccess();

        var periodStart = new DateOnly(2025, 2, 1);
        var periodEnd = new DateOnly(2025, 2, 28);
        var workInput = new PayrollWorkInput(
            20m,
            3m,
            2m,
            1m,
            1m,
            1m,
            1m,
            0m,
            0m,
            null,
            31,
            false,
            null,
            null,
            null);
        var amounts = new PayrollRecordAmountsDto(
            1_000m,
            5_000_000m,
            350_000m,
            1_350_000m,
            5_000_000m);
        var calculatedAmounts = new PayrollCalculatedAmountsDto(
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

        using (new AssertionScope())
        {
            record.Update(periodStart, periodEnd, false, 20m, 12m, 3m, 8m, workInput, amounts, calculatedAmounts)
                .IsSuccess.Should().BeFalse();
            record.EnsureCanDelete().IsSuccess.Should().BeFalse();
        }
    }
}