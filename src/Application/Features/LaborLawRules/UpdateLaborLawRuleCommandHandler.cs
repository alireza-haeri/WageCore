namespace Application.Features.LaborLawRules;

public class UpdateLaborLawRuleCommandHandler(
    ILaborLawRuleRepository laborLawRuleRepository,
    ILaborLawRuleQuery laborLawRuleQuery)
    : IRequestHandler<UpdateLaborLawRuleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateLaborLawRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await laborLawRuleRepository.GetByIdAsync(request.LaborLawRuleId, cancellationToken);
        if (rule is null)
            return Result<bool>.NotfoundFailure("قانون مورد نظر یافت نشد.");

        if (request.EffectiveFrom is not null)
        {
            var existEffectiveFrom = await laborLawRuleQuery.IsExistEffectiveFrom(
                request.EffectiveFrom.Value,
                request.LaborLawRuleId,
                cancellationToken);

            if (existEffectiveFrom)
                return Result<bool>.ValidationFailure(new Dictionary<string, string[]>
                {
                    { nameof(request.EffectiveFrom), ["تاریخ اجرا تکراری است."] }
                });
        }

        var domainResult = rule.Update(request.Key!.Value, request.Value!.Value, request.EffectiveFrom);
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await laborLawRuleRepository.UpdateAsync(rule, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در بروزرسانی قانون کار رخ داد.");

        return Result<bool>.Success(true);
    }
}
