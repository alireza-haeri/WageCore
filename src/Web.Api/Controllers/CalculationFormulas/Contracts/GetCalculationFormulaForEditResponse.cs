namespace Web.Api.Controllers.CalculationFormulas.Contracts;

public record GetCalculationFormulaForEditResponse(
    FormulaKey Key,
    string Expression,
    string EffectiveFrom);
