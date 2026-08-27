namespace Application.Features.CalculationFormulas;

public record DeleteCalculationFormulaCommand(Guid CalculationFormulaId)
    : IRequest<Result<bool>>;
