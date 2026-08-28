namespace Application.Tests.Features.PayrollRecords.Command.CreatePayrollRecord;

public class CreatePayrollRecordCommandHandlerTests
{
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly CreatePayrollRecordCommandHandler _handler;
    private readonly PayrollRecordBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    public CreatePayrollRecordCommandHandlerTests()
    {
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _handler = new CreatePayrollRecordCommandHandler(_persianCalendarService);
    }

    private CreatePayrollRecordCommand CreateValidCommand(
        int persianYear = ValidPersianYear,
        int persianMonth = ValidPersianMonth) =>
        new(
            ValidUserId,
            ValidEmployeeId,
            persianYear,
            persianMonth,
            _builder.BuildDto());

    [Fact]
    public async Task Handle_WhenPeriodStartsAfterToday_ShouldReturnGeneralFailure()
    {
        var startPeriod = DateOnly.FromDateTime(DateTime.Now).AddMonths(1);
        _persianCalendarService
            .GetMonthRange(ValidPersianYear, ValidPersianMonth)
            .Returns((startPeriod, startPeriod.AddDays(29)));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("تاریخ شروع دوره نباید برای آینده باشد.", BadResultType.General);
    }

    [Fact]
    public void Handle_WhenPeriodStartsToday_ShouldResolveThePeriodAndContinue()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        _persianCalendarService
            .GetMonthRange(ValidPersianYear, ValidPersianMonth)
            .Returns((today, today.AddDays(29)));

        var act = () => _handler.Handle(CreateValidCommand(), CancellationToken.None);

        act.Should().Throw<NotImplementedException>();
        _persianCalendarService.Received(1).GetMonthRange(ValidPersianYear, ValidPersianMonth);
    }

    [Fact]
    public void Handle_WithPastPeriod_ShouldPassYearAndMonthToTheCalendarService()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        _persianCalendarService
            .GetMonthRange(1403, 2)
            .Returns((today.AddDays(-455), today.AddDays(-425)));

        var act = () => _handler.Handle(CreateValidCommand(1403, 2), CancellationToken.None);

        act.Should().Throw<NotImplementedException>();
        _persianCalendarService.Received(1).GetMonthRange(1403, 2);
    }
}
