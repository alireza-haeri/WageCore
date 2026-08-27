namespace Application.Features.LaborLawRules;

public class GetLaborLawRuleForEditQueryHandler(ILaborLawRuleQuery laborLawRuleQuery)
    : IRequestHandler<GetLaborLawRuleForEditQuery, Result<GetLaborLawRuleForEditQueryResponse>>
{
    public async Task<Result<GetLaborLawRuleForEditQueryResponse>> Handle(
        GetLaborLawRuleForEditQuery request,
        CancellationToken cancellationToken)
    {
        var rule = await laborLawRuleQuery.GetLaborLawRuleByIdAsync(request.LaborLawRuleId, cancellationToken);
        if (rule is null)
            return Result<GetLaborLawRuleForEditQueryResponse>.NotfoundFailure("قانون مورد نظر یافت نشد.");

        return Result<GetLaborLawRuleForEditQueryResponse>.Success(
            new GetLaborLawRuleForEditQueryResponse(rule.Key, rule.Value, rule.EffectiveFrom));
    }
}
