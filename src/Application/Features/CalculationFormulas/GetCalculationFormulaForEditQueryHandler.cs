namespace Application.Features.CalculationFormulas;

public class GetCalculationFormulaForEditQueryHandler(ICalculationFormulaQuery calculationFormulaQuery)
    : IRequestHandler<GetCalculationFormulaForEditQuery, Result<GetCalculationFormulaForEditQueryResponse>>
{
    public async Task<Result<GetCalculationFormulaForEditQueryResponse>> Handle(
        GetCalculationFormulaForEditQuery request,
        CancellationToken cancellationToken)
    {
        var formula = await calculationFormulaQuery.GetCalculationFormulaByIdAsync(
            request.CalculationFormulaId,
            cancellationToken);

        if (formula is null)
            return Result<GetCalculationFormulaForEditQueryResponse>.NotfoundFailure("فرمول مورد نظر یافت نشد.");

        return Result<GetCalculationFormulaForEditQueryResponse>.Success(
            new GetCalculationFormulaForEditQueryResponse(formula.Key, formula.Expression, formula.EffectiveFrom));
    }
}
