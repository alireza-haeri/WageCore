namespace Core.Contracts.LaborLaw;

public record LaborLawRuleByIdResult(
    LaborLawRuleKey Key,
    decimal Value,
    DateOnly EffectiveFrom);
