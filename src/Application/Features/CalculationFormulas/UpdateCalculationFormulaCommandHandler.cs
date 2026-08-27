namespace Application.Features.CalculationFormulas;

public class UpdateCalculationFormulaCommandHandler(
    ICalculationFormulaRepository calculationFormulaRepository,
    ICalculationFormulaQuery calculationFormulaQuery)
    : IRequestHandler<UpdateCalculationFormulaCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateCalculationFormulaCommand request, CancellationToken cancellationToken)
    {
        var formula = await calculationFormulaRepository.GetByIdAsync(request.CalculationFormulaId, cancellationToken);
        if (formula is null)
            return Result<bool>.NotfoundFailure("فرمول مورد نظر یافت نشد.");

        if (request.EffectiveFrom is not null)
        {
            var existEffectiveFrom = await calculationFormulaQuery.IsExistEffectiveFrom(
                request.EffectiveFrom.Value,
                request.CalculationFormulaId,
                cancellationToken);

            if (existEffectiveFrom)
                return Result<bool>.ValidationFailure(new Dictionary<string, string[]>
                {
                    { nameof(request.EffectiveFrom), ["تاریخ اجرا تکراری است."] }
                });
        }

        var domainResult = formula.Update(request.Key!.Value, request.Expression, request.EffectiveFrom);
        if (!domainResult.IsSuccess)
            return Result<bool>.GeneralFailure(domainResult.ErrorMessage!);

        var updateResult = await calculationFormulaRepository.UpdateAsync(formula, cancellationToken);
        if (!updateResult)
            return Result<bool>.GeneralFailure("خطایی در بروزرسانی فرمول رخ داد.");

        return Result<bool>.Success(true);
    }
}
