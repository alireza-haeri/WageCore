namespace Web.Api.Controllers.LaborLawRules.Contracts;

public record GetLaborLawRuleForEditResponse(
    LaborLawRuleKey Key,
    decimal Value,
    string EffectiveFrom);
