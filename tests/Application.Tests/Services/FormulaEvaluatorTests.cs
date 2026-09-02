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
        decimal BaseDailySalary,
        decimal MonthlyWorkingHours,
        decimal MinimumMonthlyWage);

    private record AllowanceProfile(decimal? AttractionAllowance, int? ChildrenCount, DateOnly? EffectiveFrom);

    private record WorkInput(decimal WorkedDaysCount);

    private record WorkshopSettings(decimal MinimumMonthlyWage, decimal FridayAllowanceFactor);

    private record PayrollPeriod(DateOnly PeriodStart, TimeOnly StartTime);

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
            "[SalaryProfileBaseDailySalary] / [SalaryProfileMonthlyWorkingHours] * [WorkInputWorkedDaysCount]",
            salary,
            work);

        result.ShouldBeSuccess().Should().Be(960_000m);
    }

    [Fact]
    public void Evaluate_WithNullNullableDecimalProperty_ShouldBindItAsZero()
    {
        var profile = new AllowanceProfile(null, 0, null);

        var result = _evaluator.Evaluate(
            "[AllowanceProfileAttractionAllowance] * 2 + 1",
            profile);

        result.ShouldBeSuccess().Should().Be(1m);
    }

    [Fact]
    public void Evaluate_WithNullNullableIntegerProperty_ShouldBindItAsZero()
    {
        var profile = new AllowanceProfile(0m, null, null);

        var result = _evaluator.Evaluate(
            "[AllowanceProfileChildrenCount] * 3",
            profile);

        result.ShouldBeSuccess().Should().Be(0m);
    }

    [Fact]
    public void Evaluate_WithValueNullableDecimalProperty_ShouldBindItsValue()
    {
        var profile = new AllowanceProfile(500_000m, 0, null);

        var result = _evaluator.Evaluate(
            "[AllowanceProfileAttractionAllowance] * 2",
            profile);

        result.ShouldBeSuccess().Should().Be(1_000_000m);
    }

    [Fact]
    public void Evaluate_WithNullNullableDateOnlyProperty_ShouldLeaveItUnbound()
    {
        var profile = new AllowanceProfile(0m, 0, null);

        var result = _evaluator.Evaluate(
            "[AllowanceProfileEffectiveFrom] = [RequestedDate] ? 3 : 7",
            profile,
            new FormulaVariable("RequestedDate", new DateOnly(2025, 1, 1)));

        // The null DateOnly property is not a numeric, so it stays unbound and
        // referencing it is an evaluation failure (same as before normalization).
        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول");
    }

    [Fact]
    public void Evaluate_WithFractionalLiteral_ShouldKeepTheFractionalPart()
    {
        var work = new WorkInput(24m);

        var result = _evaluator.Evaluate("[WorkInputWorkedDaysCount] * 1.5", work);

        result.ShouldBeSuccess().Should().Be(36m);
    }

    [Fact]
    public void Evaluate_WithDecimalVariable_ShouldBindItUnderItsOwnName()
    {
        var work = new WorkInput(24m);

        var result = _evaluator.Evaluate(
            "[WorkInputWorkedDaysCount] * [DailyWage]",
            work,
            new FormulaVariable("DailyWage", 40_000m));

        result.ShouldBeSuccess().Should().Be(960_000m);
    }

    [Fact]
    public void Evaluate_WithBooleanVariable_ShouldUseItAsTheCondition()
    {
        var result = _evaluator.Evaluate(
            "[IsDraft] ? 5 : 9",
            new FormulaVariable("IsDraft", true));

        result.ShouldBeSuccess().Should().Be(5m);
    }

    [Fact]
    public void Evaluate_WithStringVariable_ShouldCompareItForEquality()
    {
        var result = _evaluator.Evaluate(
            @"[Note] = ""مأموریت"" ? 2 : 8",
            new FormulaVariable("Note", "مأموریت"));

        result.ShouldBeSuccess().Should().Be(2m);
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
    public void Evaluate_WithDateOnlyValues_ShouldCompareThemForEquality()
    {
        var period = new PayrollPeriod(new DateOnly(2025, 2, 1), new TimeOnly(8, 0));

        var result = _evaluator.Evaluate(
            "[PayrollPeriodPeriodStart] = [RequestedPeriodStart] ? 12 : 4",
            period,
            new FormulaVariable("RequestedPeriodStart", new DateOnly(2025, 2, 1)));

        result.ShouldBeSuccess().Should().Be(12m);
    }

    [Fact]
    public void Evaluate_WithTimeOnlyValues_ShouldCompareThemForEquality()
    {
        var period = new PayrollPeriod(new DateOnly(2025, 2, 1), new TimeOnly(22, 30));

        var result = _evaluator.Evaluate(
            "[PayrollPeriodStartTime] = [RequestedStartTime] ? 3 : 7",
            period,
            new FormulaVariable("RequestedStartTime", new TimeOnly(22, 30)));

        result.ShouldBeSuccess().Should().Be(3m);
    }

    [Fact]
    public void Evaluate_WhenTheSameModelIsPassedTwice_ShouldReturnFailure()
    {
        var salary = new SalaryProfile(7_200_000m, 180m, 5_000_000m);

        var result = _evaluator.Evaluate("[SalaryProfileBaseDailySalary]", salary, salary);

        result.ShouldBeFailure("نام پارامتر SalaryProfileBaseDailySalary در فرمول تکراری است.");
    }

    [Fact]
    public void Evaluate_WhenAVariableTakesAModelParameterName_ShouldReturnFailure()
    {
        var salary = new SalaryProfile(7_200_000m, 180m, 5_000_000m);

        var result = _evaluator.Evaluate(
            "[SalaryProfileBaseDailySalary]",
            salary,
            new FormulaVariable("SalaryProfileBaseDailySalary", 1m));

        result.ShouldBeFailure("نام پارامتر SalaryProfileBaseDailySalary در فرمول تکراری است.");
    }

    [Fact]
    public void Evaluate_WithNullModelsAndVariables_ShouldReturnFailure()
    {
        var result = _evaluator.Evaluate("[SalaryProfileBaseDailySalary]", null!);

        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول");
    }

    [Fact]
    public void Evaluate_WithMalformedExpression_ShouldReturnFailure()
    {
        var result = _evaluator.Evaluate("2 +* 3");

        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول");
    }
}
