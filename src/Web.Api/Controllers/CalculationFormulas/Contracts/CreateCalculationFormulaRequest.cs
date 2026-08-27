namespace Web.Api.Controllers.CalculationFormulas.Contracts;

public record CreateCalculationFormulaRequest(
    FormulaKey CalculationFormulaKey,
    string Expression,
    PersianDate EffectiveFrom);
