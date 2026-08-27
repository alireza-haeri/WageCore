using Core.Contracts;

namespace Application.Features.CalculationFormulas;

public record GetCalculationFormulasQuery(
    PaginationDto Pagination,
    FormulaKey? Key = null)
    : IRequest<Result<PagedResult<GetCalculationFormulasQueryResponse>>>;

public record GetCalculationFormulasQueryResponse(
    Guid Id,
    FormulaKey Key,
    string Expression,
    DateOnly EffectiveFrom);
