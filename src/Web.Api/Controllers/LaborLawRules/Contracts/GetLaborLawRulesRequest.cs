namespace Web.Api.Controllers.LaborLawRules.Contracts;

public record GetLaborLawRulesRequest(
    PaginationDto Pagination,
    LaborLawRuleKey? Key = null);

public record GetLaborLawRulesResponse(
    Guid Id,
    LaborLawRuleKey Key,
    decimal Value,
    string DisplayEffectiveFrom);
