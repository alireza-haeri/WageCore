namespace Application.Features.CalculationFormulas;

public class CreateCalculationFormulaCommandHandler(
    ICalculationFormulaRepository calculationFormulaRepository,
    ICalculationFormulaQuery calculationFormulaQuery)
    : IRequestHandler<CreateCalculationFormulaCommand, Result<CreateCalculationFormulaCommandResponse>>
{
    public async Task<Result<CreateCalculationFormulaCommandResponse>> Handle(
        CreateCalculationFormulaCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EffectiveFrom is not null)
        {
            var existEffectiveFrom = await calculationFormulaQuery.IsExistEffectiveFrom(
                request.EffectiveFrom.Value,
                null,
                cancellationToken);

            if (existEffectiveFrom)
                return Result<CreateCalculationFormulaCommandResponse>.ValidationFailure(new Dictionary<string, string[]>
                {
                    { nameof(request.EffectiveFrom), ["تاریخ اجرا تکراری است."] }
                });
        }

        var formulaResult = CalculationFormula.Create(
            request.Key!.Value,
            request.Expression,
            request.EffectiveFrom);

        if (!formulaResult.IsSuccess)
            return Result<CreateCalculationFormulaCommandResponse>.GeneralFailure(formulaResult.ErrorMessage!);

        var createResult = await calculationFormulaRepository.CreateAsync(formulaResult.Response!, cancellationToken);
        if (createResult is null)
            return Result<CreateCalculationFormulaCommandResponse>.GeneralFailure("خطا در ایجاد فرمول");

        return Result<CreateCalculationFormulaCommandResponse>.Success(
            new CreateCalculationFormulaCommandResponse(createResult.Value));
    }
}
