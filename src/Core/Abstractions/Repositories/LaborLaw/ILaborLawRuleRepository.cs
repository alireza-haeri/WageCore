namespace Core.Abstractions.Repositories.LaborLaw;

public interface ILaborLawRuleRepository
{
    Task<Guid?> CreateAsync(LaborLawRuleItem rule, CancellationToken cancellationToken = default);
    Task<LaborLawRuleItem?> GetByIdAsync(Guid ruleId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(LaborLawRuleItem rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid ruleId, CancellationToken cancellationToken = default);
}
