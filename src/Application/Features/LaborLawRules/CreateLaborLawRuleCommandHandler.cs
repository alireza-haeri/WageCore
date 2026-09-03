namespace Application.Features.LaborLawRules;

public class CreateLaborLawRuleCommandHandler(
    ILaborLawRuleRepository laborLawRuleRepository,
    ILaborLawRuleQuery laborLawRuleQuery)
    : IRequestHandler<CreateLaborLawRuleCommand, Result<CreateLaborLawRuleCommandResponse>>
{
    public async Task<Result<CreateLaborLawRuleCommandResponse>> Handle(
        CreateLaborLawRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EffectiveFrom is not null)
        {
            var existEffectiveFrom = await laborLawRuleQuery.IsExistEffectiveFrom(
                request.Key!.Value,
                request.EffectiveFrom.Value,
                null,
                cancellationToken);

            if (existEffectiveFrom)
                return Result<CreateLaborLawRuleCommandResponse>.ValidationFailure(new Dictionary<string, string[]>
                {
                    { nameof(request.EffectiveFrom), ["تاریخ اجرا تکراری است."] }
                });
        }

        var ruleResult = LaborLawRuleItem.Create(
            request.Key!.Value,
            request.Value!.Value,
            request.EffectiveFrom);

        if (!ruleResult.IsSuccess)
            return Result<CreateLaborLawRuleCommandResponse>.GeneralFailure(ruleResult.ErrorMessage!);

        var createResult = await laborLawRuleRepository.CreateAsync(ruleResult.Response!, cancellationToken);
        if (createResult is null)
            return Result<CreateLaborLawRuleCommandResponse>.GeneralFailure("خطا در ایجاد قانون کار");

        return Result<CreateLaborLawRuleCommandResponse>.Success(
            new CreateLaborLawRuleCommandResponse(createResult.Value));
    }
}
