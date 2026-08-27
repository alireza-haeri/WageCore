namespace Core.Abstractions.Repositories.LaborLaw;

public interface ILaborLawRuleQuery
{
    Task<decimal?> GetActiveValueAsync(
        LaborLawRuleKey key, DateOnly date, CancellationToken cancellationToken = default);
}
