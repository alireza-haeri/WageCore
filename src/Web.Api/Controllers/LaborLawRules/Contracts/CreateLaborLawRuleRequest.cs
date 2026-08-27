namespace Web.Api.Controllers.LaborLawRules.Contracts;

public record CreateLaborLawRuleRequest(
    LaborLawRuleKey LaborLawRuleKey,
    decimal Value,
    PersianDate EffectiveFrom);
