namespace Application.Tests.Features.PayrollRecords.Command.CreatePayrollRecord;

public class CreatePayrollRecordCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollCalculationService _payrollCalculationService;
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly CreatePayrollRecordCommandHandler _handler;

    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
    private static readonly DateOnly PeriodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    public CreatePayrollRecordCommandHandlerTests()
    {
        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _payrollCalculationService = Substitute.For<IPayrollCalculationService>();
        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();
        _handler = new CreatePayrollRecordCommandHandler(
            _employeeRepository,
            _persianCalendarService,
            _payrollCalculationService,
            _payrollRecordRepository);
    }

    private CreatePayrollRecordCommand CreateValidCommand(PayrollWorkInputDto? work = null) =>
        new(
            ValidUserId,
            ValidEmployeeId,
            ValidPersianYear,
            ValidPersianMonth,
            work ?? _payrollRecordBuilder.BuildDto());

    private void SetupPeriod(DateOnly startPeriod, DateOnly endPeriod) =>
        _persianCalendarService
            .GetMonthRange(ValidPersianYear, ValidPersianMonth)
            .Returns((startPeriod, endPeriod));

    private void SetupFoundEmployee(bool isTaxSubject = true) =>
        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(_employeeBuilder
                .WithId(ValidEmployeeId)
                .WithIsTaxSubject(isTaxSubject)
                .CreateResult()
                .ShouldBeSuccess());

    private void SetupCalculation(PayrollCalculationResult calculation) =>
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInputDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PayrollCalculationResult>.Success(calculation));

    private static PayrollCalculationResult ValidCalculation() =>
        new(20m, 12m, 800_000m, 300_000m, 250_000m, 1_500_000m, 15_000_000m);

    [Fact]
    public async Task Handle_WhenPeriodStartsAfterToday_ShouldReturnGeneralFailure()
    {
        var startPeriod = DateOnly.FromDateTime(DateTime.Now).AddMonths(1);
        SetupPeriod(startPeriod, startPeriod.AddDays(29));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("تاریخ شروع دوره نباید برای آینده باشد.", BadResultType.General);
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotfoundFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await _payrollCalculationService.DidNotReceive()
            .CalculateAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInputDto>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCalculationFails_ShouldReturnTheCalculationErrors()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupFoundEmployee();
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInputDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PayrollCalculationResult>.GeneralFailure("نرخ اضافه‌کاری یافت نشد."));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("نرخ اضافه‌کاری یافت نشد.", BadResultType.Validation);
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreatePayrollRecordFromTheCalculation()
    {
        var createdId = Guid.NewGuid();
        var work = _payrollRecordBuilder.BuildDto();
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupFoundEmployee();
        SetupCalculation(ValidCalculation());
        _payrollRecordRepository
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(CreateValidCommand(work), CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.PayrollRecordId.Should().Be(createdId);

        await _payrollCalculationService.Received(1).CalculateAsync(
            ValidEmployeeId,
            PeriodStart,
            PeriodEnd,
            work,
            Arg.Any<CancellationToken>());
        await _payrollRecordRepository.Received(1).CreateAsync(
            Arg.Is<PayrollRecord>(x =>
                x.EmployeeId == ValidEmployeeId &&
                x.PeriodStart == PeriodStart &&
                x.PeriodEnd == PeriodEnd &&
                x.Status == PayrollRecordStatus.Draft &&
                x.OvertimeHours == 4m &&
                x.OvertimeAmount == 800_000m &&
                x.NetPayableAmount == 15_000_000m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsTheRecord_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupFoundEmployee();
        SetupCalculation(new PayrollCalculationResult(
            2m,
            12m,
            800_000m,
            300_000m,
            250_000m,
            1_500_000m,
            15_000_000m));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure(
            "ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.",
            BadResultType.General);
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }
}
