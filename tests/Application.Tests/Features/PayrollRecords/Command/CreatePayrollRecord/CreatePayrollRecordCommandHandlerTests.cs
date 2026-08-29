namespace Application.Tests.Features.PayrollRecords.Command.CreatePayrollRecord;

public class CreatePayrollRecordCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly IWorkShopRepository _workShopRepository;
    private readonly IEmployeeSalaryProfileQuery _employeeSalaryProfileQuery;
    private readonly IPayrollCalculationService _payrollCalculationService;
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly CreatePayrollRecordCommandHandler _handler;

    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();
    private readonly Employee _employee;
    private readonly Workshop _workshop;
    private readonly IReadOnlyList<EmployeeSalaryProfile> _salaryProfiles;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
    private static readonly DateOnly PeriodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    public CreatePayrollRecordCommandHandlerTests()
    {
        _employee = new EmployeeBuilder()
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .CreateResult()
            .ShouldBeSuccess();
        _workshop = new WorkshopBuilder()
            .WithId(ValidWorkshopId)
            .WithUserId(ValidUserId)
            .CreateResult()
            .ShouldBeSuccess();
        _salaryProfiles =
        [
            new EmployeeSalaryProfileBuilder()
                .WithEmployeeId(ValidEmployeeId)
                .CreateResult()
                .ShouldBeSuccess()
        ];

        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _employeeSalaryProfileQuery = Substitute.For<IEmployeeSalaryProfileQuery>();
        _payrollCalculationService = Substitute.For<IPayrollCalculationService>();
        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();

        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(_employee);
        _workShopRepository
            .GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(_workshop);
        _employeeSalaryProfileQuery
            .GetEmployeeSalaryProfilesAffectingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(_salaryProfiles);
        SetupCalculation(ValidCalculation());

        _handler = new CreatePayrollRecordCommandHandler(
            _employeeRepository,
            _persianCalendarService,
            _payrollRecordQuery,
            _workShopRepository,
            _employeeSalaryProfileQuery,
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

    private void SetupCalculation(PayrollCalculationResult calculation) =>
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<EmployeeSalaryProfile>>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInputDto>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PayrollCalculationResult>.Success(calculation));

    private void SetupOverlappingPeriod() =>
        _payrollRecordQuery
            .HasOverlappingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                PeriodEnd,
                null,
                Arg.Any<CancellationToken>())
            .Returns(true);

    private Task DidNotReceiveOverlapCheck() =>
        _payrollRecordQuery.DidNotReceive()
            .HasOverlappingPeriodAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>());

    private Task DidNotReceiveCalculation() =>
        _payrollCalculationService.DidNotReceive()
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<EmployeeSalaryProfile>>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInputDto>(),
                Arg.Any<CancellationToken>());

    private Task DidNotReceiveSalaryProfiles() =>
        _employeeSalaryProfileQuery.DidNotReceive()
            .GetEmployeeSalaryProfilesAffectingPeriodAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>());

    private static PayrollCalculationResult ValidCalculation() =>
        new(20m, 12m, 800_000m, 300_000m, 250_000m, 1_500_000m, 15_000_000m);

    [Fact]
    public async Task Handle_WhenPeriodStartsAfterToday_ShouldReturnGeneralFailure()
    {
        var startPeriod = DateOnly.FromDateTime(DateTime.Now).AddMonths(1);
        SetupPeriod(startPeriod, startPeriod.AddDays(29));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("تاریخ شروع دوره نباید برای آینده باشد.", BadResultType.General);
        await DidNotReceiveOverlapCheck();
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await DidNotReceiveSalaryProfiles();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotfoundFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveOverlapCheck();
        await DidNotReceiveSalaryProfiles();
        await DidNotReceiveCalculation();
    }

    [Fact]
    public async Task Handle_WhenWorkshopNotFound_ShouldReturnNotfoundFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _workShopRepository
            .GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns((Workshop?)null);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("کارگاه مورد نظر یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveCalculation();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoSalaryProfileAffectsThePeriod_ShouldReturnNotfoundFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _employeeSalaryProfileQuery
            .GetEmployeeSalaryProfilesAffectingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                PeriodEnd,
                Arg.Any<CancellationToken>())
            .Returns(new List<EmployeeSalaryProfile>());

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("برای این بازه پروفایل حقوقی کارمند یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveCalculation();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAnotherPayrollRecordOverlapsThePeriod_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupOverlappingPeriod();

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("برای این کارمند در این بازه فیش پرداختی دیگری ثبت شده است.", BadResultType.General);
        await DidNotReceiveSalaryProfiles();
        await DidNotReceiveCalculation();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAskForOverlapsWithoutExcludingAnyRecord()
    {
        SetupPeriod(PeriodStart, PeriodEnd);

        await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        await _payrollRecordQuery.Received(1).HasOverlappingPeriodAsync(
            ValidUserId,
            ValidEmployeeId,
            PeriodStart,
            PeriodEnd,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLoadTheSalaryProfilesOfTheResolvedPeriod()
    {
        SetupPeriod(PeriodStart, PeriodEnd);

        await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfilesAffectingPeriodAsync(
            ValidUserId,
            ValidEmployeeId,
            PeriodStart,
            PeriodEnd,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCalculationFails_ShouldReturnTheCalculationErrors()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<EmployeeSalaryProfile>>(),
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
        _payrollRecordRepository
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(CreateValidCommand(work), CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.PayrollRecordId.Should().Be(createdId);

        await _payrollCalculationService.Received(1).CalculateAsync(
            _employee,
            _workshop,
            _salaryProfiles,
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
