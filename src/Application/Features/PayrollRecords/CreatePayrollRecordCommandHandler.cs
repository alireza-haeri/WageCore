namespace Application.Features.PayrollRecords;

public class CreatePayrollRecordCommandHandler(IPersianCalendarService persianCalendarService)
    : IRequestHandler<CreatePayrollRecordCommand, Result<CreatePayrollRecordCommandResponse>>
{
    public Task<Result<CreatePayrollRecordCommandResponse>> Handle(
        CreatePayrollRecordCommand request,
        CancellationToken cancellationToken)
    {
        var period = persianCalendarService.GetMonthRange(request.PersianYear, request.PersianMonth);

        if (period.StartPeriod > DateOnly.FromDateTime(DateTime.Now))
            return Task.FromResult(
                Result<CreatePayrollRecordCommandResponse>.GeneralFailure("تاریخ شروع دوره نباید برای آینده باشد."));

        // TODO: Not implemented yet.
        throw new NotImplementedException();
    }
}
