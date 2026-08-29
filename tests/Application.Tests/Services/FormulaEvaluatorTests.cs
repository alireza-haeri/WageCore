using Microsoft.Extensions.Logging;

namespace Application.Tests.Services;

public class FormulaEvaluatorTests
{
    private readonly FormulaEvaluator _evaluator;

    public FormulaEvaluatorTests()
    {
        _evaluator = new FormulaEvaluator(Substitute.For<ILogger<FormulaEvaluator>>());
    }

    private record SalaryProfile(decimal BaseMonthlySalary, decimal MonthlyWorkingHours);

    private record WorkInput(decimal WorkedDaysCount);

    [Fact]
    public void Evaluate_WithConstantExpression_ShouldFollowOperatorsPrecedence()
    {
        var result = _evaluator.Evaluate("2 + 3 * 4");

        result.ShouldBeSuccess().Should().Be(14m);
    }

    [Fact]
    public void Evaluate_WithGroupedExpression_ShouldEvaluateTheParenthesesFirst()
    {
        var result = _evaluator.Evaluate("(2 + 3) * 4");

        result.ShouldBeSuccess().Should().Be(20m);
    }

    [Fact]
    public void Evaluate_WithExpressionOverSeveralModels_ShouldBindEveryPropertyByName()
    {
        var salary = new SalaryProfile(7_200_000m, 180m);
        var work = new WorkInput(24m);

        var result = _evaluator.Evaluate(
            "[BaseMonthlySalary] / [MonthlyWorkingHours] * [WorkedDaysCount]",
            salary,
            work);

        result.ShouldBeSuccess().Should().Be(960_000m);
    }

    [Fact]
    public void Evaluate_WithFractionalLiteral_ShouldKeepTheFractionalPart()
    {
        var work = new WorkInput(24m);

        var result = _evaluator.Evaluate("[WorkedDaysCount] * 1.5", work);

        result.ShouldBeSuccess().Should().Be(36m);
    }

    [Fact]
    public void Evaluate_WithNullModels_ShouldReturnFailure()
    {
        var result = _evaluator.Evaluate("[BaseMonthlySalary]", null!);

        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول");
    }

    [Fact]
    public void Evaluate_WithMalformedExpression_ShouldReturnFailure()
    {
        var result = _evaluator.Evaluate("2 +* 3");

        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول");
    }
}
