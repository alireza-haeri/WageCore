using Application.Features.PayrollRecords.Query.GetPayrollRecords;
using Core.Contracts;
using Core.Contracts.PayrollRecords;
using NSubstitute;
using Shared.Kernel.Common;

namespace Application.Tests.Features.PayrollRecords.Query.GetPayrollRecords;

public class GetPayrollRecordsQueryHandlerTests
{
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly GetPayrollRecordsQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    private static readonly DateOnly MonthStart = new(2025, 6, 20);
    private static readonly DateOnly MonthEnd = new(2025, 7, 19);

    public GetPayrollRecordsQueryHandlerTests()
    {
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();

        _handler = new GetPayrollRecordsQueryHandler(
            _persianCalendarService,
            _payrollRecordQuery);
    }

    private GetPayrollRecordsQuery CreateValidQuery(
        int? persianYear = null,
        int? persianMonth = null) =>
        new(
            ValidUserId,
            new PaginationDto(1, 10),
            null,
            null,
            null,
            persianYear,
            persianMonth);

    private void SetupMonthRange(int year, int month, DateOnly start, DateOnly end) =>
        _persianCalendarService
            .GetMonthRange(year, month)
            .Returns((start, end));

    [Fact]
    public async Task Handle_WithPersianYearAndMonth_ShouldPassTheMonthRangeToTheQuery()
    {
        SetupMonthRange(ValidPersianYear, ValidPersianMonth, MonthStart, MonthEnd);
        _payrollRecordQuery
            .GetPayrollRecordsAsync(
                ValidUserId,
                Arg.Any<PaginationDto>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<Guid?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PayrollRecordResult>([], 0, 1, 10));

        var result = await _handler.Handle(
            CreateValidQuery(ValidPersianYear, ValidPersianMonth),
            CancellationToken.None);

        result.ShouldBeSuccess();
        await _payrollRecordQuery.Received(1).GetPayrollRecordsAsync(
            ValidUserId,
            Arg.Any<PaginationDto>(),
            null,
            null,
            null,
            MonthStart,
            MonthEnd,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPersianYearOnly_ShouldPassTheWholeYearRangeToTheQuery()
    {
        var yearStart = new DateOnly(2025, 3, 20);
        var yearEnd = new DateOnly(2026, 3, 19);
        SetupMonthRange(ValidPersianYear, 1, yearStart, MonthEnd);
        SetupMonthRange(ValidPersianYear, 12, MonthStart, yearEnd);
        _payrollRecordQuery
            .GetPayrollRecordsAsync(
                Arg.Any<Guid>(),
                Arg.Any<PaginationDto>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<Guid?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PayrollRecordResult>([], 0, 1, 10));

        var result = await _handler.Handle(
            CreateValidQuery(persianYear: ValidPersianYear),
            CancellationToken.None);

        result.ShouldBeSuccess();
        await _payrollRecordQuery.Received(1).GetPayrollRecordsAsync(
            ValidUserId,
            Arg.Any<PaginationDto>(),
            null,
            null,
            null,
            yearStart,
            yearEnd,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutPersianFilters_ShouldPassNoPeriodRangeToTheQuery()
    {
        _payrollRecordQuery
            .GetPayrollRecordsAsync(
                Arg.Any<Guid>(),
                Arg.Any<PaginationDto>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<Guid?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PayrollRecordResult>([], 0, 1, 10));

        var result = await _handler.Handle(CreateValidQuery(), CancellationToken.None);

        result.ShouldBeSuccess();
        _persianCalendarService.DidNotReceive()
            .GetMonthRange(Arg.Any<int>(), Arg.Any<int>());
        await _payrollRecordQuery.Received(1).GetPayrollRecordsAsync(
            ValidUserId,
            Arg.Any<PaginationDto>(),
            null,
            null,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassTheFiltersThroughToTheQuery()
    {
        var workshopId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        _payrollRecordQuery
            .GetPayrollRecordsAsync(
                Arg.Any<Guid>(),
                Arg.Any<PaginationDto>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<Guid?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<PayrollRecordResult>([], 0, 1, 10));

        var query = new GetPayrollRecordsQuery(
            ValidUserId,
            new PaginationDto(2, 20),
            "رضا",
            workshopId,
            departmentId,
            null,
            null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeSuccess();
        await _payrollRecordQuery.Received(1).GetPayrollRecordsAsync(
            ValidUserId,
            new PaginationDto(2, 20),
            "رضا",
            workshopId,
            departmentId,
            null,
            null,
            Arg.Any<CancellationToken>());
    }
}
