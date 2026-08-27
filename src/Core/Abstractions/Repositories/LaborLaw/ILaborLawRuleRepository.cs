namespace Core.Abstractions.Repositories.LaborLaw;

public interface ILaborLawRuleRepository
{
    Task<Guid?> CreateAsync(LaborLawRuleItem rule, CancellationToken cancellationToken = default);
}
