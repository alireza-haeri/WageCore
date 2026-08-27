namespace Application.Features.CalculationFormulas;

public record CreateCalculationFormulaCommand(
    FormulaKey? Key,
    string Expression,
    DateOnly? EffectiveFrom)
    : IRequest<Result<CreateCalculationFormulaCommandResponse>>;

public record CreateCalculationFormulaCommandResponse(Guid CalculationFormulaId);
