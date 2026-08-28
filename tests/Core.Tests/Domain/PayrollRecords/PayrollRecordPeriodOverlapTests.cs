namespace Core.Tests.Domain.PayrollRecords;

public class PayrollRecordPeriodOverlapTests
{
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);

    private readonly PayrollRecordBuilder _builder = new();

    [Theory]
    [InlineData(-40, -10, false)] // کاملاً قبل از دوره
    [InlineData(-1, 5, true)] // یک روز همپوشانی در ابتدای دوره
    [InlineData(0, 0, true)] // یک روزه و منطبق با شروع دوره
    [InlineData(5, 10, true)] // داخل دوره
    [InlineData(-100, 100, true)] // شامل کل دوره
    [InlineData(30, 60, true)] // یک روز همپوشانی در پایان دوره
    [InlineData(31, 60, false)] // کاملاً بعد از دوره
    public void HasOverlap_ShouldDetectOverlappingPeriods(
        int otherStartOffset,
        int otherEndOffset,
        bool expected)
    {
        var otherPeriodStart = PeriodStart.AddDays(otherStartOffset);
        var otherPeriodEnd = PeriodStart.AddDays(otherEndOffset);

        var result = PayrollRecord.HasOverlap(
            PeriodStart,
            PeriodEnd,
            otherPeriodStart,
            otherPeriodEnd);

        result.Should().Be(expected);
    }

    [Fact]
    public void HasOverlap_ShouldBeSymmetric()
    {
        var otherPeriodStart = PeriodEnd;
        var otherPeriodEnd = PeriodEnd.AddDays(10);

        PayrollRecord.HasOverlap(
                PeriodStart,
                PeriodEnd,
                otherPeriodStart,
                otherPeriodEnd)
            .Should()
            .Be(PayrollRecord.HasOverlap(
                otherPeriodStart,
                otherPeriodEnd,
                PeriodStart,
                PeriodEnd));
    }

    [Fact]
    public void HasOverlap_WithSingleDayPeriods_ShouldOnlyMatchTheSameDay()
    {
        using (new AssertionScope())
        {
            PayrollRecord.HasOverlap(PeriodStart, PeriodStart, PeriodStart, PeriodStart)
                .Should().BeTrue();

            PayrollRecord.HasOverlap(PeriodStart, PeriodStart, PeriodStart.AddDays(1), PeriodStart.AddDays(1))
                .Should().BeFalse();
        }
    }

    [Fact]
    public void RecordHasOverlap_WithOverlappingPeriod_ShouldReturnTrue()
    {
        var record = _builder
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

        using (new AssertionScope())
        {
            record.HasOverlap(PeriodEnd, PeriodEnd.AddDays(10)).Should().BeTrue();
            record.HasOverlap(PeriodStart.AddDays(1), PeriodEnd.AddDays(-1)).Should().BeTrue();
        }
    }

    [Fact]
    public void RecordHasOverlap_WithAdjacentPeriods_ShouldReturnFalse()
    {
        var record = _builder
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

        using (new AssertionScope())
        {
            record.HasOverlap(PeriodStart.AddDays(-10), PeriodStart.AddDays(-1)).Should().BeFalse();
            record.HasOverlap(PeriodEnd.AddDays(1), PeriodEnd.AddDays(10)).Should().BeFalse();
        }
    }
}
