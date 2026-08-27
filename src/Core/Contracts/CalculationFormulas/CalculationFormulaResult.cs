namespace Core.Contracts.CalculationFormulas;

public record CalculationFormulaResult(
    Guid Id,
    FormulaKey Key,
    string Expression,
    DateOnly EffectiveFrom);
