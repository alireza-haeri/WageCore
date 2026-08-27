namespace Web.Api.Controllers.CalculationFormulas.Contracts;

public record UpdateCalculationFormulaRequest(
    FormulaKey CalculationFormulaKey,
    string Expression,
    PersianDate EffectiveFrom);
