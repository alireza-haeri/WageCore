using Core.Contracts;

namespace Application.Features.LaborLawRules;

public class GetLaborLawRulesQueryHandler(ILaborLawRuleQuery laborLawRuleQuery)
    : IRequestHandler<GetLaborLawRulesQuery, Result<PagedResult<GetLaborLawRulesQueryResponse>>>
{
    public async Task<Result<PagedResult<GetLaborLawRulesQueryResponse>>> Handle(
        GetLaborLawRulesQuery request,
        CancellationToken cancellationToken)
    {
        var pagedRules = await laborLawRuleQuery.GetLaborLawRulesAsync(
            request.Pagination,
            request.Key,
            cancellationToken);

        var response = pagedRules.Map(x =>
            new GetLaborLawRulesQueryResponse(x.Id, x.Key, x.Value, x.EffectiveFrom));

        return Result<PagedResult<GetLaborLawRulesQueryResponse>>.Success(response);
    }
}
