using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers.PayrollRecords;

[Authorize]
[Tags("PayrollRecord")]
[Route("api/v1/payroll-records")]
public class PayrollRecordController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreatePayrollRecord")]
    public async Task<ActionResult<Result<CreatePayrollRecordCommandResponse>>> CreatePayrollRecord(
        [FromBody] CreatePayrollRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreatePayrollRecordCommand(
            UserId: UserId,
            EmployeeId: request.EmployeeId,
            PersianYear: request.PersianYear,
            PersianMonth: request.PersianMonth,
            Work: request.Work
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{payrollRecordId:guid}")]
    [SwaggerOperation(OperationId = "UpdatePayrollRecord")]
    public async Task<ActionResult<Result<bool>>> UpdatePayrollRecord(
        Guid payrollRecordId,
        [FromBody] UpdatePayrollRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdatePayrollRecordCommand(
            UserId: UserId,
            EmployeeId: request.EmployeeId,
            PayrollRecordId: payrollRecordId,
            PersianYear: request.PersianYear,
            PersianMonth: request.PersianMonth,
            Work: request.Work
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{payrollRecordId:guid}")]
    [SwaggerOperation(OperationId = "DeletePayrollRecord")]
    public async Task<ActionResult<Result<bool>>> DeletePayrollRecord(
        [FromQuery] Guid employeeId,
        Guid payrollRecordId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeletePayrollRecordCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            PayrollRecordId: payrollRecordId
        ), cancellationToken);

        return Result(result);
    }

    [HttpPost("{payrollRecordId:guid}/mark-as-paid")]
    [SwaggerOperation(OperationId = "MarkPayrollRecordAsPaid")]
    public async Task<ActionResult<Result<bool>>> MarkPayrollRecordAsPaid(
        [FromQuery] Guid employeeId,
        Guid payrollRecordId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MarkPayrollRecordAsPaidCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            PayrollRecordId: payrollRecordId
        ), cancellationToken);

        return Result(result);
    }
}
