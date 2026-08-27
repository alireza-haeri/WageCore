namespace Application.Features.LaborLawRules;

public record DeleteLaborLawRuleCommand(Guid LaborLawRuleId)
    : IRequest<Result<bool>>;
