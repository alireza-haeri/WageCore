namespace Application.Features.LaborLawRules;

public class CreateLaborLawRuleCommandHandler(ILaborLawRuleRepository laborLawRuleRepository)
    : IRequestHandler<CreateLaborLawRuleCommand, Result<CreateLaborLawRuleCommandResponse>>
{
    public async Task<Result<CreateLaborLawRuleCommandResponse>> Handle(
        CreateLaborLawRuleCommand request,
        CancellationToken cancellationToken)
    {
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
