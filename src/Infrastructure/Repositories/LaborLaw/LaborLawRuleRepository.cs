namespace Infrastructure.Repositories.LaborLaw;

public class LaborLawRuleRepository(WageCoreDbContext context, ILogger<LaborLawRuleRepository> logger)
    : ILaborLawRuleRepository
{
    public async Task<Guid?> CreateAsync(LaborLawRuleItem rule, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.LaborLawRuleItems.AddAsync(rule, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return rule.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while creating a labor law rule for Key: {Key}.", rule.Key);
            return null;
        }
    }

    public async Task<LaborLawRuleItem?> GetByIdAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await context.LaborLawRuleItems
            .FirstOrDefaultAsync(x => x.Id == ruleId, cancellationToken);
    }

    public async Task<bool> UpdateAsync(LaborLawRuleItem rule, CancellationToken cancellationToken = default)
    {
        try
        {
            context.LaborLawRuleItems.Update(rule);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while updating a labor law rule for Id: {RuleId}.", rule.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var rule = await context.LaborLawRuleItems
                .FirstOrDefaultAsync(x => x.Id == ruleId, cancellationToken);
            if (rule is null)
                return false;

            context.LaborLawRuleItems.Remove(rule);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while deleting a labor law rule for Id: {RuleId}.", ruleId);
            return false;
        }
    }
}
