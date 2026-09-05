namespace Core.Abstractions.Repositories.LaborLaw;

public interface ILaborLawRuleQuery
{
    Task<decimal?> GetActiveValueAsync(
        LaborLawRuleKey key, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the active value of every configured labor law rule as of
    /// <paramref name="date"/> in a single query. For each key the value of the
    /// latest rule with <c>EffectiveFrom</c> on or before the date wins; keys
    /// without a rule on or before the date are absent from the result.
    /// </summary>
    Task<IReadOnlyDictionary<LaborLawRuleKey, decimal>> GetActiveRuleValuesAsync(
        DateOnly date, CancellationToken cancellationToken = default);

    Task<PagedResult<LaborLawRuleResult>> GetLaborLawRulesAsync(
        PaginationDto pagination,
        LaborLawRuleKey? key = null,
        CancellationToken cancellationToken = default);

    Task<LaborLawRuleByIdResult?> GetLaborLawRuleByIdAsync(
        Guid ruleId,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistEffectiveFrom(
        LaborLawRuleKey key,
        DateOnly effectiveFrom,
        Guid? excludeRuleId = null,
        CancellationToken cancellationToken = default);
}
