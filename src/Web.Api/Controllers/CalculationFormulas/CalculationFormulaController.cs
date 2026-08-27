using Microsoft.AspNetCore.Authorization;
using Shared.Web.DateTimeHandling;
using Application.Features.CalculationFormulas;

namespace Web.Api.Controllers.CalculationFormulas;

[Authorize(Roles = ApplicationRoles.SiteManagerRule)]
[Tags("CalculationFormula")]
[Route("api/v1/calculation-formulas")]
public class CalculationFormulaController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateCalculationFormula")]
    public async Task<ActionResult<Result<CreateCalculationFormulaCommandResponse>>> CreateCalculationFormula(
        [FromBody] CreateCalculationFormulaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCalculationFormulaCommand(
            Key: request.Key,
            Expression: request.Expression,
            EffectiveFrom: request.EffectiveFrom.ToDateOnly()
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{calculationFormulaId:guid}")]
    [SwaggerOperation(OperationId = "UpdateCalculationFormula")]
    public async Task<ActionResult<Result<bool>>> UpdateCalculationFormula(
        [FromBody] UpdateCalculationFormulaRequest request,
        Guid calculationFormulaId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCalculationFormulaCommand(
            CalculationFormulaId: calculationFormulaId,
            Key: request.Key,
            Expression: request.Expression,
            EffectiveFrom: request.EffectiveFrom.ToDateOnly()
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{calculationFormulaId:guid}")]
    [SwaggerOperation(OperationId = "DeleteCalculationFormula")]
    public async Task<ActionResult<Result<bool>>> DeleteCalculationFormula(
        Guid calculationFormulaId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new DeleteCalculationFormulaCommand(calculationFormulaId),
            cancellationToken);

        return Result(result);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetCalculationFormulas")]
    public async Task<ActionResult<Result<PagedResult<GetCalculationFormulasResponse>>>> GetCalculationFormulas(
        [FromQuery] GetCalculationFormulasRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCalculationFormulasQuery(
            Pagination: request.Pagination,
            Key: request.Key
        ), cancellationToken);

        var response = result
            .Map(paged => paged
                .Map(f => new GetCalculationFormulasResponse(
                    f.Id,
                    f.Key,
                    f.Expression,
                    PersianDate.FromDateOnly(f.EffectiveFrom).ToDisplay(UserPersianDateFormat)
                ))
            );

        return Result(response);
    }

    [HttpGet("{calculationFormulaId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetCalculationFormulaForEdit")]
    public async Task<ActionResult<Result<GetCalculationFormulaForEditResponse>>> GetCalculationFormulaForEdit(
        Guid calculationFormulaId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCalculationFormulaForEditQuery(calculationFormulaId),
            cancellationToken);

        var response = result.Map(f => new GetCalculationFormulaForEditResponse(
            f.Key,
            f.Expression,
            PersianDate.ToRawValue(f.EffectiveFrom)
        ));

        return Result(response);
    }
}
