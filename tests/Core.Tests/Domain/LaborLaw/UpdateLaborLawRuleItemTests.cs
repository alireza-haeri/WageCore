using Core.Domain.Enums;

namespace Core.Tests.Domain.LaborLaw;

public class UpdateLaborLawRuleItemTests
{
    private readonly LaborLawRuleItemBuilder _builder = new();

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var rule = _builder.CreateResult().ShouldBeSuccess();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));

        var result = rule.Update(LaborLawRuleKey.MinimumDailySalary, 103_909_680m, effectiveFrom);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            rule.Key.Should().Be(LaborLawRuleKey.MinimumDailySalary);
            rule.Value.Should().Be(103_909_680m);
            rule.EffectiveFrom.Should().Be(effectiveFrom);
        }
    }

    [Fact]
    public void Update_WithNegativeValue_ShouldFail()
    {
        var rule = _builder.CreateResult().ShouldBeSuccess();

        var result = rule.Update(LaborLawRuleKey.MinimumDailySalary, -1, DateOnly.FromDateTime(DateTime.Now));

        result.ShouldBeFailure("مقدار قانون نمیتواند منفی باشد.");
    }

    [Fact]
    public void Update_WithNullEffectiveFrom_ShouldFail()
    {
        var rule = _builder.CreateResult().ShouldBeSuccess();

        var result = rule.Update(LaborLawRuleKey.MinimumDailySalary, 71_661_840m, null);

        result.ShouldBeFailure("تاریخ اجرا نمیتواند خالی باشد.");
    }

    [Fact]
    public void Update_WhenFailed_ShouldNotChangeValues()
    {
        var rule = _builder.WithValue(71_661_840m).CreateResult().ShouldBeSuccess();
        var originalValue = rule.Value;
        var originalEffectiveFrom = rule.EffectiveFrom;

        rule.Update(LaborLawRuleKey.MinimumDailySalary, -10, DateOnly.FromDateTime(DateTime.Now));

        using (new AssertionScope())
        {
            rule.Value.Should().Be(originalValue);
            rule.EffectiveFrom.Should().Be(originalEffectiveFrom);
        }
    }
}
