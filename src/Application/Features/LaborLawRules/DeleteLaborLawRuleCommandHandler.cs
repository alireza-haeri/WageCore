namespace Application.Features.LaborLawRules;

public class DeleteLaborLawRuleCommandHandler(ILaborLawRuleRepository laborLawRuleRepository)
    : IRequestHandler<DeleteLaborLawRuleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteLaborLawRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await laborLawRuleRepository.GetByIdAsync(request.LaborLawRuleId, cancellationToken);
        if (rule is null)
            return Result<bool>.NotfoundFailure("قانون مورد نظر یافت نشد.");

        var deleteResult = await laborLawRuleRepository.DeleteAsync(request.LaborLawRuleId, cancellationToken);
        if (!deleteResult)
            return Result<bool>.GeneralFailure("خطایی در حذف قانون کار رخ داد.");

        return Result<bool>.Success(true);
    }
}
