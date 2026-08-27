namespace Web.Api.Controllers.LaborLawRules.Contracts;

public record GetLaborLawRulesRequest(
    PaginationDto Pagination,
    LaborLawRuleKey? LaborLawRuleKey = null);

public record GetLaborLawRulesResponse(
    Guid Id,
    LaborLawRuleKey Key,
    decimal Value,
    string DisplayEffectiveFrom);
