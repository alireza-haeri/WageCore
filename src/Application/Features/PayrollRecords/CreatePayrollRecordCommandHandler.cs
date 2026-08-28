namespace Application.Features.PayrollRecords;

public class CreatePayrollRecordCommandHandler
    : IRequestHandler<CreatePayrollRecordCommand, Result<CreatePayrollRecordCommandResponse>>
{
    public Task<Result<CreatePayrollRecordCommandResponse>> Handle(
        CreatePayrollRecordCommand request,
        CancellationToken cancellationToken)
    {
        // TODO: Not implemented yet.
        throw new NotImplementedException();
    }
}
