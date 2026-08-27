namespace Application.Features.LaborLawRules;

public record UpdateLaborLawRuleCommand(
    Guid LaborLawRuleId,
    LaborLawRuleKey? Key,
    decimal? Value,
    DateOnly? EffectiveFrom)
    : IRequest<Result<bool>>;
