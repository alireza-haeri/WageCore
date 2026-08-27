namespace Web.Api.Controllers.LaborLawRules.Contracts;

public record UpdateLaborLawRuleRequest(
    LaborLawRuleKey Key,
    decimal Value,
    PersianDate EffectiveFrom);
