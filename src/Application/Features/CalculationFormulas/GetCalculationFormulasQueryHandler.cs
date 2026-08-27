using Core.Contracts;

namespace Application.Features.CalculationFormulas;

public class GetCalculationFormulasQueryHandler(ICalculationFormulaQuery calculationFormulaQuery)
    : IRequestHandler<GetCalculationFormulasQuery, Result<PagedResult<GetCalculationFormulasQueryResponse>>>
{
    public async Task<Result<PagedResult<GetCalculationFormulasQueryResponse>>> Handle(
        GetCalculationFormulasQuery request,
        CancellationToken cancellationToken)
    {
        var pagedFormulas = await calculationFormulaQuery.GetCalculationFormulasAsync(
            request.Pagination,
            request.Key,
            cancellationToken);

        var response = pagedFormulas.Map(x =>
            new GetCalculationFormulasQueryResponse(x.Id, x.Key, x.Expression, x.EffectiveFrom));

        return Result<PagedResult<GetCalculationFormulasQueryResponse>>.Success(response);
    }
}
