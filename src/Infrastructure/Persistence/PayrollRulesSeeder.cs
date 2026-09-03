namespace Infrastructure.Persistence;

/// <summary>
/// Seeds the reference payroll data (labor law rules and calculation formulas)
/// for the Persian year 1400 so a development database works out of the box.
/// It is idempotent: an entry that already exists for the seed date is skipped.
/// </summary>
public class PayrollRulesSeeder(
    ILaborLawRuleRepository laborLawRuleRepository,
    ICalculationFormulaRepository calculationFormulaRepository,
    ILaborLawRuleQuery laborLawRuleQuery,
    ICalculationFormulaQuery calculationFormulaQuery,
    IPersianCalendarService persianCalendarService,
    ILogger<PayrollRulesSeeder> logger)
{
    public const int SeededPersianYear = 1400;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var (effectiveFrom, _) = persianCalendarService.GetMonthRange(SeededPersianYear, 1);

        foreach (var (key, value) in PayrollFormulaCatalog.RuleValues)
        {
            var exists = await laborLawRuleQuery.IsExistEffectiveFrom(
                key, effectiveFrom, null, cancellationToken);
            if (exists)
                continue;

            var ruleResult = LaborLawRuleItem.Create(key, value, effectiveFrom);
            if (!ruleResult.IsSuccess)
            {
                logger.LogError(
                    "Failed to build labor law rule {Key}: {Error}", key, ruleResult.ErrorMessage);
                throw new InvalidOperationException(ruleResult.ErrorMessage);
            }

            var createdId = await laborLawRuleRepository.CreateAsync(
                ruleResult.Response!, cancellationToken);
            if (createdId is null)
            {
                logger.LogError("Failed to persist labor law rule {Key}.", key);
                throw new InvalidOperationException($"خطا در ایجاد قانون کار {key}.");
            }

            logger.LogInformation(
                "Seeded labor law rule {Key} effective from {EffectiveFrom}.",
                key, effectiveFrom);
        }

        foreach (var (key, expression) in PayrollFormulaCatalog.Formulas)
        {
            var exists = await calculationFormulaQuery.IsExistEffectiveFrom(
                key, effectiveFrom, null, cancellationToken);
            if (exists)
                continue;

            var formulaResult = CalculationFormula.Create(key, expression, effectiveFrom);
            if (!formulaResult.IsSuccess)
            {
                logger.LogError(
                    "Failed to build calculation formula {Key}: {Error}", key, formulaResult.ErrorMessage);
                throw new InvalidOperationException(formulaResult.ErrorMessage);
            }

            var createdId = await calculationFormulaRepository.CreateAsync(
                formulaResult.Response!, cancellationToken);
            if (createdId is null)
            {
                logger.LogError("Failed to persist calculation formula {Key}.", key);
                throw new InvalidOperationException($"خطا در ایجاد فرمول {key}.");
            }

            logger.LogInformation(
                "Seeded calculation formula {Key} effective from {EffectiveFrom}.",
                key, effectiveFrom);
        }
    }
}
