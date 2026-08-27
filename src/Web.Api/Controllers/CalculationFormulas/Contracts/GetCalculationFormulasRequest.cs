namespace Web.Api.Controllers.CalculationFormulas.Contracts;

public record GetCalculationFormulasRequest(
    PaginationDto Pagination,
    FormulaKey? Key = null);

public record GetCalculationFormulasResponse(
    Guid Id,
    FormulaKey Key,
    string Expression,
    string DisplayEffectiveFrom);
