using Core.Contracts;

namespace Application.Features.LaborLawRules;

public record GetLaborLawRulesQuery(
    PaginationDto Pagination,
    LaborLawRuleKey? Key = null)
    : IRequest<Result<PagedResult<GetLaborLawRulesQueryResponse>>>;

public record GetLaborLawRulesQueryResponse(
    Guid Id,
    LaborLawRuleKey Key,
    decimal Value,
    DateOnly EffectiveFrom);
