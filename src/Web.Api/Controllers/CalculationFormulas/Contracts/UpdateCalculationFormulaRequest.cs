namespace Web.Api.Controllers.CalculationFormulas.Contracts;

public record UpdateCalculationFormulaRequest(
    FormulaKey Key,
    string Expression,
    PersianDate EffectiveFrom);
