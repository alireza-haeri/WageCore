namespace Web.Api.Controllers.CalculationFormulas.Contracts;

public record CreateCalculationFormulaRequest(
    FormulaKey Key,
    string Expression,
    PersianDate EffectiveFrom);
