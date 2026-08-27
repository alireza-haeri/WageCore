namespace Core.Contracts.CalculationFormulas;

public record CalculationFormulaByIdResult(
    FormulaKey Key,
    string Expression,
    DateOnly EffectiveFrom);
