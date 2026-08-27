namespace Core.Abstractions.Repositories.LaborLaw;

public interface ILaborLawRuleQuery
{
    Task<decimal?> GetActiveValueAsync(
        LaborLawRuleKey key, DateOnly date, CancellationToken cancellationToken = default);

    Task<PagedResult<LaborLawRuleResult>> GetLaborLawRulesAsync(
        PaginationDto pagination,
        LaborLawRuleKey? key = null,
        CancellationToken cancellationToken = default);

    Task<LaborLawRuleByIdResult?> GetLaborLawRuleByIdAsync(
        Guid ruleId,
        CancellationToken cancellationToken = default);

    Task<bool> IsExistEffectiveFrom(
        DateOnly effectiveFrom,
        Guid? excludeRuleId = null,
        CancellationToken cancellationToken = default);
}
