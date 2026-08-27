namespace Application.Features.LaborLawRules;

public class UpdateLaborLawRuleCommandHandler(ILaborLawRuleRepository laborLawRuleRepository)
    : IRequestHandler<UpdateLaborLawRuleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateLaborLawRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await laborLawRuleRepository.GetByIdAsync(request.LaborLawRuleId, cancellationToken);
        if (rule is null)
            return Result<bool>.NotfoundFailure("قانون مورد نظر یافت نشد.");

        var domainResult = rule.Update(request.Key!.Value, request.Value!.Value, request.EffectiveFrom);
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await laborLawRuleRepository.UpdateAsync(rule, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در بروزرسانی قانون کار رخ داد.");

        return Result<bool>.Success(true);
    }
}
