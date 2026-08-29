namespace Application.Tests.Features.PayrollRecords.Command.UpdatePayrollRecord;

public class UpdatePayrollRecordCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly IWorkShopRepository _workShopRepository;
    private readonly IEmployeeSalaryProfileQuery _employeeSalaryProfileQuery;
    private readonly IPayrollCalculationService _payrollCalculationService;
    private readonly UpdatePayrollRecordCommandHandler _handler;

    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();
    private readonly Employee _employee;
    private readonly Workshop _workshop;
    private readonly IReadOnlyList<EmployeeSalaryProfile> _salaryProfiles;
    private readonly PayrollRecord _payrollRecord;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
    private static readonly DateOnly PeriodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    public UpdatePayrollRecordCommandHandlerTests()
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
        _payrollRecord = _payrollRecordBuilder
            .WithId(ValidPayrollRecordId)
            .WithEmployeeId(ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();

        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _employeeSalaryProfileQuery = Substitute.For<IEmployeeSalaryProfileQuery>();
        _payrollCalculationService = Substitute.For<IPayrollCalculationService>();

        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns(_payrollRecord);
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
        _payrollRecordRepository
            .UpdateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>())
            .Returns(true);
        SetupCalculation(UpdatedCalculation());

        _handler = new UpdatePayrollRecordCommandHandler(
            _employeeRepository,
            _persianCalendarService,
            _payrollRecordQuery,
            _payrollRecordRepository,
            _workShopRepository,
            _employeeSalaryProfileQuery,
            _payrollCalculationService);
    }

    private UpdatePayrollRecordCommand CreateValidCommand(PayrollWorkInputDto? work = null) =>
        new(
            ValidUserId,
            ValidEmployeeId,
            ValidPayrollRecordId,
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

    private void SetupRecord(PayrollRecord payrollRecord) =>
        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns(payrollRecord);

    private void SetupEmployee(Employee employee) =>
        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

    private PayrollRecord CreatePaidRecord()
    {
        var paidRecord = _payrollRecordBuilder
            .WithId(ValidPayrollRecordId)
            .WithEmployeeId(ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();
        paidRecord.MarkAsPaid().ShouldBeSuccess();

        return paidRecord;
    }

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

    private Task DidNotReceiveUpdate() =>
        _payrollRecordRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());

    private static PayrollCalculationResult UpdatedCalculation() =>
        new(20m, 12m, 100_000m, 50_000m, 25_000m, 10_000m, 900_000m);

    [Fact]
    public async Task Handle_WhenPeriodStartsAfterToday_ShouldReturnGeneralFailure()
    {
        var startPeriod = DateOnly.FromDateTime(DateTime.Now).AddMonths(1);
        SetupPeriod(startPeriod, startPeriod.AddDays(29));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("تاریخ شروع دوره نباید برای آینده باشد.", BadResultType.General);
        await _payrollRecordRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordNotFound_ShouldReturnNotfoundFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _payrollRecordRepository
            .GetByIdAsync(ValidUserId, ValidPayrollRecordId, Arg.Any<CancellationToken>())
            .Returns((PayrollRecord?)null);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی مورد نظر یافت نشد.", BadResultType.NotFound);
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordBelongsToAnotherEmployee_ShouldReturnNotfoundFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupRecord(_payrollRecordBuilder
            .WithId(ValidPayrollRecordId)
            .WithEmployeeId(Guid.NewGuid())
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess());

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی مورد نظر یافت نشد.", BadResultType.NotFound);
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenPayrollRecordIsPaid_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupRecord(CreatePaidRecord());

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("فیش پرداختی پرداخت شده قابل ویرایش نیست.", BadResultType.General);
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await DidNotReceiveOverlapCheck();
        await DidNotReceiveCalculation();
        await DidNotReceiveUpdate();
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
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenEmployeeWasHiredAfterThePeriod_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupEmployee(new EmployeeBuilder()
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithHireDate(PeriodEnd.AddDays(1))
            .CreateResult()
            .ShouldBeSuccess());

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("کارمند در این بازه استخدام نشده بود.", BadResultType.General);
        await DidNotReceiveOverlapCheck();
        await DidNotReceiveCalculation();
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenEmployeeTerminatedBeforeThePeriod_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        var terminatedEmployee = new EmployeeBuilder()
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithWorkshopRegistrationDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-90)))
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-60)))
            .CreateResult()
            .ShouldBeSuccess();
        terminatedEmployee.Terminate(PeriodStart.AddDays(-3)).ShouldBeSuccess();
        SetupEmployee(terminatedEmployee);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("کارمند قبل از این بازه ترک کار کرده است.", BadResultType.General);
        await DidNotReceiveCalculation();
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenAnotherPayrollRecordOverlapsThePeriod_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _payrollRecordQuery
            .HasOverlappingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                PeriodEnd,
                ValidPayrollRecordId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("برای این کارمند در این بازه فیش پرداختی دیگری ثبت شده است.", BadResultType.General);
        await DidNotReceiveCalculation();
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_ShouldExcludeTheUpdatedRecordFromTheOverlapCheck()
    {
        SetupPeriod(PeriodStart, PeriodEnd);

        await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        await _payrollRecordQuery.Received(1).HasOverlappingPeriodAsync(
            ValidUserId,
            ValidEmployeeId,
            PeriodStart,
            PeriodEnd,
            ValidPayrollRecordId,
            Arg.Any<CancellationToken>());
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
        await DidNotReceiveUpdate();
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

        result.ShouldBeFailure("برای این بازه حکم حقوقی کارمند یافت نشد.", BadResultType.NotFound);
        await DidNotReceiveCalculation();
        await DidNotReceiveUpdate();
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
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsTheUpdate_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupCalculation(new PayrollCalculationResult(
            2m,
            12m,
            100_000m,
            50_000m,
            25_000m,
            10_000m,
            900_000m));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure(
            "ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.",
            BadResultType.General);
        await DidNotReceiveUpdate();
    }

    [Fact]
    public async Task Handle_WhenRepositoryFailsToSave_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _payrollRecordRepository
            .UpdateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("خطا در بروزرسانی فیش پرداختی", BadResultType.General);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateTheLoadedRecordFromTheCalculation()
    {
        var work = _payrollRecordBuilder.BuildDto();
        SetupPeriod(PeriodStart, PeriodEnd);

        var result = await _handler.Handle(CreateValidCommand(work), CancellationToken.None);

        result.ShouldBeSuccess().Should().BeTrue();

        await _payrollCalculationService.Received(1).CalculateAsync(
            _employee,
            _workshop,
            _salaryProfiles,
            PeriodStart,
            PeriodEnd,
            work,
            Arg.Any<CancellationToken>());
        await _payrollRecordRepository.Received(1).UpdateAsync(
            Arg.Is<PayrollRecord>(x =>
                x == _payrollRecord &&
                x.Id == ValidPayrollRecordId &&
                x.EmployeeId == ValidEmployeeId &&
                x.PeriodStart == PeriodStart &&
                x.PeriodEnd == PeriodEnd &&
                x.Status == PayrollRecordStatus.Draft &&
                x.OvertimeHours == 4m &&
                x.OvertimeAmount == 100_000m &&
                x.NetPayableAmount == 900_000m),
            Arg.Any<CancellationToken>());
    }
}
