using Core.Domain.Enums;

namespace Core.Tests.Domain.CalculationFormulas;

public class UpdateCalculationFormulaTests
{
    private readonly CalculationFormulaBuilder _builder = new();

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var formula = _builder.CreateResult().ShouldBeSuccess();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
        var expression = "OvertimeHours * HourlyRate * 1.5";

        var result = formula.Update(FormulaKey.OvertimePay, expression, effectiveFrom);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            formula.Key.Should().Be(FormulaKey.OvertimePay);
            formula.Expression.Should().Be(expression);
            formula.EffectiveFrom.Should().Be(effectiveFrom);
        }
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Update_WithNullOrWhiteSpaceExpression_ShouldFail(string? expression)
    {
        var formula = _builder.CreateResult().ShouldBeSuccess();

        var result = formula.Update(FormulaKey.OvertimePay, expression!, DateOnly.FromDateTime(DateTime.Now));

        result.ShouldBeFailure("عبارت فرمول نمیتواند خالی باشد.");
    }

    [Fact]
    public void Update_WithNullEffectiveFrom_ShouldFail()
    {
        var formula = _builder.CreateResult().ShouldBeSuccess();

        var result = formula.Update(FormulaKey.OvertimePay, "Hours * Rate", null);

        result.ShouldBeFailure("تاریخ اجرا نمیتواند خالی باشد.");
    }

    [Fact]
    public void Update_WhenFailed_ShouldNotChangeValues()
    {
        var formula = _builder.WithExpression("Hours * Rate").CreateResult().ShouldBeSuccess();
        var originalExpression = formula.Expression;
        var originalEffectiveFrom = formula.EffectiveFrom;

        formula.Update(FormulaKey.OvertimePay, "   ", DateOnly.FromDateTime(DateTime.Now));

        using (new AssertionScope())
        {
            formula.Expression.Should().Be(originalExpression);
            formula.EffectiveFrom.Should().Be(originalEffectiveFrom);
        }
    }
}
