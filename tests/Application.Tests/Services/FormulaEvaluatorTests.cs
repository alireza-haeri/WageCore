using Microsoft.Extensions.Logging;

namespace Application.Tests.Services;

public class FormulaEvaluatorTests
{
    private readonly FormulaEvaluator _evaluator;

    public FormulaEvaluatorTests()
    {
        _evaluator = new FormulaEvaluator(Substitute.For<ILogger<FormulaEvaluator>>());
    }

    private record SalaryProfile(
        decimal BaseMonthlySalary,
        decimal MonthlyWorkingHours,
        decimal MinimumMonthlyWage);

    private record WorkInput(decimal WorkedDaysCount);

    private record WorkshopSettings(decimal MinimumMonthlyWage, decimal FridayAllowanceFactor);

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
        var salary = new SalaryProfile(7_200_000m, 180m, 4_000_000m);
        var work = new WorkInput(24m);

        var result = _evaluator.Evaluate(
            "[SalaryProfileBaseMonthlySalary] / [SalaryProfileMonthlyWorkingHours] * [WorkInputWorkedDaysCount]",
            salary,
            work);

        result.ShouldBeSuccess().Should().Be(960_000m);
    }

    [Fact]
    public void Evaluate_WithFractionalLiteral_ShouldKeepTheFractionalPart()
    {
        var work = new WorkInput(24m);

        var result = _evaluator.Evaluate("[WorkInputWorkedDaysCount] * 1.5", work);

        result.ShouldBeSuccess().Should().Be(36m);
    }

    [Fact]
    public void Evaluate_WhenSeveralModelsShareAPropertyName_ShouldKeepBothParameters()
    {
        var salary = new SalaryProfile(7_200_000m, 180m, 5_000_000m);
        var workshopSettings = new WorkshopSettings(4_500_000m, 1.25m);

        var result = _evaluator.Evaluate(
            "[SalaryProfileMinimumMonthlyWage] + [WorkshopSettingsMinimumMonthlyWage]",
            salary,
            workshopSettings);

        result.ShouldBeSuccess().Should().Be(9_500_000m);
    }

    [Fact]
    public void Evaluate_WhenTheSameModelIsPassedTwice_ShouldReturnFailure()
    {
        var salary = new SalaryProfile(7_200_000m, 180m, 5_000_000m);

        var result = _evaluator.Evaluate("[SalaryProfileBaseMonthlySalary]", salary, salary);

        result.ShouldBeFailure("نام پارامتر SalaryProfileBaseMonthlySalary در فرمول تکراری است.");
    }

    [Fact]
    public void Evaluate_WithNullModels_ShouldReturnFailure()
    {
        var result = _evaluator.Evaluate("[SalaryProfileBaseMonthlySalary]", null!);

        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول");
    }

    [Fact]
    public void Evaluate_WithMalformedExpression_ShouldReturnFailure()
    {
        var result = _evaluator.Evaluate("2 +* 3");

        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول");
    }
}
