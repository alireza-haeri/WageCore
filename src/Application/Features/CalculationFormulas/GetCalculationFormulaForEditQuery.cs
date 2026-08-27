namespace Application.Features.CalculationFormulas;

public record GetCalculationFormulaForEditQuery(Guid CalculationFormulaId)
    : IRequest<Result<GetCalculationFormulaForEditQueryResponse>>;

public record GetCalculationFormulaForEditQueryResponse(
    FormulaKey Key,
    string Expression,
    DateOnly EffectiveFrom);
