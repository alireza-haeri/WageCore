using Microsoft.AspNetCore.Authorization;
using Shared.Web.DateTimeHandling;
using Application.Features.LaborLawRules;
using Core.Domain.Enums;

namespace Web.Api.Controllers.LaborLawRules;

[Authorize(Roles = ApplicationRoles.SiteManagerRule)]
[Tags("LaborLawRule")]
[Route("api/v1/labor-law-rules")]
public class LaborLawRuleController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateLaborLawRule")]
    public async Task<ActionResult<Result<CreateLaborLawRuleCommandResponse>>> CreateLaborLawRule(
        [FromBody] CreateLaborLawRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateLaborLawRuleCommand(
            Key: request.Key,
            Value: request.Value,
            EffectiveFrom: request.EffectiveFrom.ToDateOnly()
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{laborLawRuleId:guid}")]
    [SwaggerOperation(OperationId = "UpdateLaborLawRule")]
    public async Task<ActionResult<Result<bool>>> UpdateLaborLawRule(
        [FromBody] UpdateLaborLawRuleRequest request,
        Guid laborLawRuleId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateLaborLawRuleCommand(
            LaborLawRuleId: laborLawRuleId,
            Key: request.Key,
            Value: request.Value,
            EffectiveFrom: request.EffectiveFrom.ToDateOnly()
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{laborLawRuleId:guid}")]
    [SwaggerOperation(OperationId = "DeleteLaborLawRule")]
    public async Task<ActionResult<Result<bool>>> DeleteLaborLawRule(
        Guid laborLawRuleId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteLaborLawRuleCommand(laborLawRuleId), cancellationToken);

        return Result(result);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetLaborLawRules")]
    public async Task<ActionResult<Result<PagedResult<GetLaborLawRulesResponse>>>> GetLaborLawRules(
        [FromQuery] GetLaborLawRulesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLaborLawRulesQuery(
            Pagination: request.Pagination,
            Key: request.Key
        ), cancellationToken);

        var response = result
            .Map(paged => paged
                .Map(r => new GetLaborLawRulesResponse(
                    r.Id,
                    r.Key,
                    r.Value,
                    PersianDate.FromDateOnly(r.EffectiveFrom).ToDisplay(UserPersianDateFormat)
                ))
            );

        return Result(response);
    }

    [HttpGet("{laborLawRuleId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetLaborLawRuleForEdit")]
    public async Task<ActionResult<Result<GetLaborLawRuleForEditResponse>>> GetLaborLawRuleForEdit(
        Guid laborLawRuleId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLaborLawRuleForEditQuery(laborLawRuleId), cancellationToken);

        var response = result.Map(r => new GetLaborLawRuleForEditResponse(
            r.Key,
            r.Value,
            PersianDate.ToRawValue(r.EffectiveFrom)
        ));

        return Result(response);
    }
}
