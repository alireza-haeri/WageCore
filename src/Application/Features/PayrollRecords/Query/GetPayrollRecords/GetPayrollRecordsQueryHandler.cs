using Core.Contracts;

namespace Application.Features.PayrollRecords.Query.GetPayrollRecords;

public class GetPayrollRecordsQueryHandler(
    IPersianCalendarService persianCalendarService,
    IPayrollRecordQuery payrollRecordQuery)
    : IRequestHandler<GetPayrollRecordsQuery, Result<PagedResult<PayrollRecordResult>>>
{
    public async Task<Result<PagedResult<PayrollRecordResult>>> Handle(
        GetPayrollRecordsQuery request,
        CancellationToken cancellationToken)
    {
        (DateOnly? periodStart, DateOnly? periodEnd) =
            ResolvePeriodRange(request.PersianYear, request.PersianMonth);

        var pagedPayrollRecords = await payrollRecordQuery.GetPayrollRecordsAsync(
            request.UserId,
            request.Pagination,
            request.Search,
            request.WorkshopId,
            request.DepartmentId,
            periodStart,
            periodEnd,
            cancellationToken);

        return Result<PagedResult<PayrollRecordResult>>.Success(pagedPayrollRecords);
    }

    /// <summary>
    /// Resolves the optional Persian year/month filter to the Gregorian range the
    /// query searches on. A year without a month covers the whole Persian year.
    /// </summary>
    private (DateOnly?, DateOnly?) ResolvePeriodRange(int? persianYear, int? persianMonth)
    {
        if (persianYear is not { } year)
            return (null, null);

        if (persianMonth is not { } month)
        {
            var (yearStart, _) = persianCalendarService.GetMonthRange(year, 1);
            var (_, yearEnd) = persianCalendarService.GetMonthRange(year, 12);
            return (yearStart, yearEnd);
        }

        return persianCalendarService.GetMonthRange(year, month);
    }
}
