using Microsoft.Extensions.Logging;

namespace Application.Tests.Services;

// Evaluates the real TaxPay expression (PayrollFormulaCatalog) against the real
// FormulaEvaluator with the bracket rule values passed as formula variables —
// proving the progressive calculation works for inside-bracket values, exact
// threshold boundaries, and rule value changes, all without touching the
// expression text.
public class PayrollTaxFormulaTests
{
    private static readonly FormulaEvaluator Evaluator =
        new(Substitute.For<ILogger<FormulaEvaluator>>());

    private static readonly LaborLawRuleKey[] TaxBracketRuleKeys =
    [
        LaborLawRuleKey.TaxBracket1Threshold,
        LaborLawRuleKey.TaxBracket2Threshold,
        LaborLawRuleKey.TaxBracket2Rate,
        LaborLawRuleKey.TaxBracket3Threshold,
        LaborLawRuleKey.TaxBracket3Rate,
        LaborLawRuleKey.TaxBracket4Threshold,
        LaborLawRuleKey.TaxBracket4Rate,
        LaborLawRuleKey.TaxBracket5Threshold,
        LaborLawRuleKey.TaxBracket5Rate,
        LaborLawRuleKey.TaxBracket6Rate
    ];

    public static TheoryData<decimal, decimal> ProgressiveTaxCases => new()
    {
        // Below the first threshold nothing is taxed.
        { 0m, 0m },
        { 39_999_999m, 0m },
        { 40_000_000m, 0m },

        // Second bracket (10% on the portion above 40,000,000).
        { 40_000_001m, 0.1m },
        { 60_000_000m, 2_000_000m },
        { 80_000_000m, 4_000_000m },
        { 80_000_001m, 4_000_000.1m },

        // Third bracket (15% on the portion above 80,000,000).
        { 90_000_000m, 5_500_000m },
        { 100_000_000m, 7_000_000m },
        { 100_000_001m, 7_000_000.15m },

        // Fourth bracket (20% on the portion above 100,000,000).
        { 110_000_000m, 9_000_000m },
        { 120_000_000m, 11_000_000m },
        { 120_000_001m, 11_000_000.2m },

        // Fifth bracket (25% on the portion above 120,000,000).
        { 130_000_000m, 13_500_000m },
        { 140_000_000m, 16_000_000m },
        { 140_000_001m, 16_000_000.25m },

        // Top open-ended bracket (30% on the portion above 140,000,000).
        { 150_000_000m, 19_000_000m },
        { 200_000_000m, 34_000_000m }
    };

    [Theory]
    [MemberData(nameof(ProgressiveTaxCases))]
    public void TaxPayFormula_WithSeedBracketRules_ShouldApplyTheProgressiveBrackets(decimal taxableAmount, decimal expectedTax)
    {
        var tax = EvaluateTax(taxableAmount, PayrollFormulaCatalog.RuleValues);

        tax.Should().Be(expectedTax);
    }

    [Fact]
    public void TaxPayFormula_WhenOnlyTheSecondBracketRateRuleChanges_ShouldChangeTheTax()
    {
        var originalTax = EvaluateTax(60_000_000m, PayrollFormulaCatalog.RuleValues);
        var changedRuleValues = new Dictionary<LaborLawRuleKey, decimal>(PayrollFormulaCatalog.RuleValues)
        {
            [LaborLawRuleKey.TaxBracket2Rate] = 20m
        };
        var taxWithChangedRule = EvaluateTax(60_000_000m, changedRuleValues);

        originalTax.Should().Be(2_000_000m);
        taxWithChangedRule.Should().Be(4_000_000m);
    }

    [Fact]
    public void TaxPayFormula_WhenAThresholdRuleChanges_ShouldChangeTheTax()
    {
        var originalTax = EvaluateTax(100_000_000m, PayrollFormulaCatalog.RuleValues);
        var changedRuleValues = new Dictionary<LaborLawRuleKey, decimal>(PayrollFormulaCatalog.RuleValues)
        {
            [LaborLawRuleKey.TaxBracket2Threshold] = 60_000_000m
        };
        var taxWithChangedRule = EvaluateTax(100_000_000m, changedRuleValues);

        originalTax.Should().Be(7_000_000m);
        // Bracket 2 now covers 40,000,000..60,000,000 and bracket 3 starts at
        // 60,000,000: (60M-40M)*10% + (100M-60M)*15% = 2M + 6M.
        taxWithChangedRule.Should().Be(8_000_000m);
    }

    private static decimal EvaluateTax(decimal taxableAmount, IReadOnlyDictionary<LaborLawRuleKey, decimal> ruleValues)
    {
        var inputs = TaxBracketRuleKeys
            .Select(key => new FormulaVariable(key.ToString(), ruleValues[key]))
            .Cast<object>()
            .ToList();
        inputs.Insert(0, new FormulaVariable("TaxableAmount", taxableAmount));

        var result = Evaluator.Evaluate(PayrollFormulaCatalog.Expression(FormulaKey.TaxPay), inputs.ToArray());

        return result.ShouldBeSuccess();
    }
}
