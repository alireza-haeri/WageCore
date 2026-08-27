namespace Web.Api.Controllers.CalculationFormulas.Contracts;

public record GetCalculationFormulasRequest(
    PaginationDto Pagination,
    FormulaKey? CalculationFormulaKey = null);

public record GetCalculationFormulasResponse(
    Guid Id,
    FormulaKey Key,
    string Expression,
    string DisplayEffectiveFrom);
