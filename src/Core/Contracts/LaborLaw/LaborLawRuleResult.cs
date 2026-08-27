namespace Core.Contracts.LaborLaw;

public record LaborLawRuleResult(
    Guid Id,
    LaborLawRuleKey Key,
    decimal Value,
    DateOnly EffectiveFrom);
