namespace Application.Features.LaborLawRules;

public record GetLaborLawRuleForEditQuery(Guid LaborLawRuleId)
    : IRequest<Result<GetLaborLawRuleForEditQueryResponse>>;

public record GetLaborLawRuleForEditQueryResponse(
    LaborLawRuleKey Key,
    decimal Value,
    DateOnly EffectiveFrom);
