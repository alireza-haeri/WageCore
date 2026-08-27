namespace Web.Api.Controllers.LaborLawRules.Contracts;

public record CreateLaborLawRuleRequest(
    LaborLawRuleKey Key,
    decimal Value,
    PersianDate EffectiveFrom);
