namespace Application.Features.LaborLawRules;

public record CreateLaborLawRuleCommand(
    LaborLawRuleKey? Key,
    decimal? Value,
    DateOnly? EffectiveFrom)
    : IRequest<Result<CreateLaborLawRuleCommandResponse>>;

public record CreateLaborLawRuleCommandResponse(Guid LaborLawRuleId);
