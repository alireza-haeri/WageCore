using Core.Domain.Enums;

namespace Core.Tests.Domain.CalculationFormulas;

public class CreateCalculationFormulaTests
{
    private readonly CalculationFormulaBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.Key.Should().Be(FormulaKey.OvertimePay);
            response.Expression.Should().Be("OvertimeHours * HourlyRate * 1.4");
            response.EffectiveFrom.Should().NotBe(default);
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var expression = "Hours * Rate";

        var result = _builder
            .WithId(id)
            .WithKey(FormulaKey.OvertimePay)
            .WithExpression(expression)
            .WithEffectiveFrom(effectiveFrom)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.Key.Should().Be(FormulaKey.OvertimePay);
            response.Expression.Should().Be(expression);
            response.EffectiveFrom.Should().Be(effectiveFrom);
        }
    }

    [Fact]
    public void Create_WithGeneratedId_ShouldReturnSuccess()
    {
        var effectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var result = CalculationFormula.Create(
            FormulaKey.OvertimePay,
            "OvertimeHours * HourlyRate * 1.4",
            effectiveFrom);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.Key.Should().Be(FormulaKey.OvertimePay);
            response.Expression.Should().Be("OvertimeHours * HourlyRate * 1.4");
            response.EffectiveFrom.Should().Be(effectiveFrom);
        }
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();

        result.ShouldBeFailure("شناسه فرمول نمیتواند خالی باشد.");
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWhiteSpaceExpression_ShouldFail(string? expression)
    {
        var result = _builder.WithExpression(expression!).CreateResult();

        result.ShouldBeFailure("عبارت فرمول نمیتواند خالی باشد.");
    }

    [Fact]
    public void Create_WithNullEffectiveFrom_ShouldFail()
    {
        var result = _builder.WithEffectiveFrom(null).CreateResult();

        result.ShouldBeFailure("تاریخ اجرا نمیتواند خالی باشد.");
    }
}
