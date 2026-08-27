namespace Application.Features.CalculationFormulas;

public record UpdateCalculationFormulaCommand(
    Guid CalculationFormulaId,
    FormulaKey? Key,
    string Expression,
    DateOnly? EffectiveFrom)
    : IRequest<Result<bool>>;
