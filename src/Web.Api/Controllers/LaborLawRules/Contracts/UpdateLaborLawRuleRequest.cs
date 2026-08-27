namespace Web.Api.Controllers.LaborLawRules.Contracts;

public record UpdateLaborLawRuleRequest(
    LaborLawRuleKey LaborLawRuleKey,
    decimal Value,
    PersianDate EffectiveFrom);
