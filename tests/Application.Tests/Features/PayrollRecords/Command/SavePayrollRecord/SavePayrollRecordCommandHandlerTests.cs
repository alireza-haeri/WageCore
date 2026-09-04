using Application.Features.PayrollRecords;
using Core.Contracts.PayrollRecords;
using Core.Domain;
using Core.Domain.Enums;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using Shared.Tests.Builders;

namespace Application.Tests.Features.PayrollRecords.Command.SavePayrollRecord;

public class SavePayrollRecordCommandHandlerTests
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollLimitsResolver _payrollLimitsResolver;
    private readonly IWorkShopRepository _workShopRepository;
    private readonly ISalaryDecreeQuery _salaryDecreeQuery;
    private readonly IPayrollCalculationService _payrollCalculationService;
    private readonly IPayrollRecordRepository _payrollRecordRepository;
    private readonly SavePayrollRecordCommandHandler _handler;

    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();
    private readonly Employee _employee;
    private readonly Workshop _workshop;
    private readonly IReadOnlyList<SalaryDecree> _salaryProfiles;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
    private static readonly DateOnly PeriodEnd = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const int ValidPersianYear = 1404;
    private const int ValidPersianMonth = 6;

    public SavePayrollRecordCommandHandlerTests()
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
            new SalaryDecreeBuilder()
                .WithEmployeeId(ValidEmployeeId)
                .CreateResult()
                .ShouldBeSuccess()
        ];

        _employeeRepository = Substitute.For<IEmployeeRepository>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _payrollLimitsResolver = Substitute.For<IPayrollLimitsResolver>();
        _workShopRepository = Substitute.For<IWorkShopRepository>();
        _salaryDecreeQuery = Substitute.For<ISalaryDecreeQuery>();
        _payrollCalculationService = Substitute.For<IPayrollCalculationService>();
        _payrollRecordRepository = Substitute.For<IPayrollRecordRepository>();

        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(_employee);
        _workShopRepository
            .GetByIdAsync(ValidUserId, ValidWorkshopId, Arg.Any<CancellationToken>())
            .Returns(_workshop);
        _salaryDecreeQuery
            .GetSalaryDecreesAffectingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(_salaryProfiles);
        _payrollRecordRepository
            .GetByEmployeeAndPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns((PayrollRecord?)null);
        SetupLimits(ValidLimits());
        SetupCalculation(ValidCalculation());

        _handler = new SavePayrollRecordCommandHandler(
            _persianCalendarService,
            _payrollLimitsResolver,
            _payrollRecordRepository,
            _employeeRepository,
            _workShopRepository,
            _salaryDecreeQuery,
            _payrollCalculationService);
    }

    private SavePayrollRecordCommand CreateValidCommand(UserWorkInputDto? work = null) =>
        new(
            ValidUserId,
            ValidEmployeeId,
            ValidPersianYear,
            ValidPersianMonth,
            work ?? _payrollRecordBuilder.BuildUserWorkInputDto());

    private void SetupPeriod(DateOnly startPeriod, DateOnly endPeriod) =>
        _persianCalendarService
            .GetMonthRange(ValidPersianYear, ValidPersianMonth)
            .Returns((startPeriod, endPeriod));

    private void SetupLimits(PayrollLimits limits) =>
        _payrollLimitsResolver
            .ResolveAsync(PeriodStart, PeriodEnd, Arg.Any<CancellationToken>())
            .Returns(Result<PayrollLimits>.Success(limits));

    private static PayrollLimits ValidLimits() =>
        new(20m, 12m, 8m, 8m);

    private void SetupExistingRecord(PayrollRecord record) =>
        _payrollRecordRepository
            .GetByEmployeeAndPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns(record);

    private void SetupCalculation(PayrollCalculationResult calculation) =>
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<SalaryDecree>>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInput>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PayrollCalculationResult>.Success(calculation));

    private void SetupCalculationFailure(string message) =>
        _payrollCalculationService
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<SalaryDecree>>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInput>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PayrollCalculationResult>.GeneralFailure(message));

    private Task DidNotReceiveCalculation() =>
        _payrollCalculationService.DidNotReceive()
            .CalculateAsync(
                Arg.Any<Employee>(),
                Arg.Any<Workshop>(),
                Arg.Any<IReadOnlyList<SalaryDecree>>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<PayrollWorkInput>(),
                Arg.Any<CancellationToken>());

    private Task DidNotReceiveSalaryProfiles() =>
        _salaryDecreeQuery.DidNotReceive()
            .GetSalaryDecreesAffectingPeriodAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>());

    private static PayrollCalculationResult ValidCalculation() =>
        new(
            new PayrollCalculatedAmountsDto(
                10_000_000m,
                0m,
                0m,
                300_000m,
                0m,
                0m,
                0m,
                0m,
                0m,
                800_000m,
                0m,
                0m,
                250_000m,
                0m,
                0m,
                0m,
                null,
                null),
            new PayrollRecordAmountsDto(
                1_500_000m,
                17_900_000m,
                1_400_000m,
                2_900_000m,
                15_000_000m),
            false);

    [Fact]
    public async Task Handle_WhenPeriodStartsAfterToday_ShouldReturnGeneralFailure()
    {
        var startPeriod = DateOnly.FromDateTime(DateTime.Now).AddMonths(1);
        SetupPeriod(startPeriod, startPeriod.AddDays(29));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("تاریخ شروع دوره نباید برای آینده باشد.", BadResultType.General);
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await DidNotReceiveSalaryProfiles();
        await _payrollRecordRepository.DidNotReceive()
            .GetByEmployeeAndPeriodAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheLimitsCannotBeResolved_ShouldReturnTheResolverErrors()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _payrollLimitsResolver
            .ResolveAsync(PeriodStart, PeriodEnd, Arg.Any<CancellationToken>())
            .Returns(Result<PayrollLimits>.NotfoundFailure("سقف ساعات اضافه‌کاری ماهانه یافت نشد."));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("سقف ساعات اضافه‌کاری ماهانه یافت نشد.", BadResultType.Validation);
        await _employeeRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await DidNotReceiveSalaryProfiles();
        DidNotReceiveCalculation();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheCalculationFails_ShouldReturnTheCalculationErrors()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupCalculationFailure("خطا در محاسبه‌ی فرمول: [BaseDailySalary] یافت نشد.");

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("خطا در محاسبه‌ی فرمول: [BaseDailySalary] یافت نشد.", BadResultType.Validation);
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
        await DidNotReceiveSalaryProfiles();
        DidNotReceiveCalculation();
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
        DidNotReceiveCalculation();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoSalaryProfileAffectsThePeriod_ShouldReturnNotfoundFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _salaryDecreeQuery
            .GetSalaryDecreesAffectingPeriodAsync(
                ValidUserId,
                ValidEmployeeId,
                PeriodStart,
                PeriodEnd,
                Arg.Any<CancellationToken>())
            .Returns(new List<SalaryDecree>());

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("برای این بازه حکم حقوقی کارمند یافت نشد.", BadResultType.NotFound);
        DidNotReceiveCalculation();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeWasHiredAfterThePeriod_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(new EmployeeBuilder()
                .WithId(ValidEmployeeId)
                .WithWorkshopId(ValidWorkshopId)
                .WithHireDate(PeriodEnd.AddDays(1))
                .CreateResult()
                .ShouldBeSuccess());

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("کارمند در این بازه استخدام نشده بود.", BadResultType.General);
        await DidNotReceiveSalaryProfiles();
        await _workShopRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        DidNotReceiveCalculation();
    }

    [Fact]
    public async Task Handle_WhenEmployeeTerminatedBeforeThePeriod_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        var employee = new EmployeeBuilder()
            .WithId(ValidEmployeeId)
            .WithWorkshopId(ValidWorkshopId)
            .WithWorkshopRegistrationDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-90)))
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-60)))
            .CreateResult()
            .ShouldBeSuccess();
        employee.Terminate(PeriodStart.AddDays(-3)).ShouldBeSuccess();
        _employeeRepository
            .GetByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("کارمند قبل از این بازه ترک کار کرده است.", BadResultType.General);
        await DidNotReceiveSalaryProfiles();
        DidNotReceiveCalculation();
    }

    [Fact]
    public async Task Handle_WhenExistingRecordIsPaid_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        var existingRecord = _payrollRecordBuilder
            .WithId(Guid.NewGuid())
            .WithEmployeeId(ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();
        existingRecord.MarkAsPaid().ShouldBeSuccess();
        SetupExistingRecord(existingRecord);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure("این فیش پرداخت شده است و قابل ویرایش نیست.", BadResultType.General);
        await DidNotReceiveSalaryProfiles();
        DidNotReceiveCalculation();
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingDraftRecord_ShouldUpdateItAndNotCreateANewRecord()
    {
        var existingId = Guid.NewGuid();
        SetupPeriod(PeriodStart, PeriodEnd);
        var existingRecord = _payrollRecordBuilder
            .WithId(existingId)
            .WithEmployeeId(ValidEmployeeId)
            .WithPeriod(PeriodStart, PeriodEnd)
            .CreateResult()
            .ShouldBeSuccess();
        SetupExistingRecord(existingRecord);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.PayrollRecordId.Should().Be(existingId);
            existingRecord.NetPayableAmount.Should().Be(15_000_000m);
            existingRecord.OvertimeAmount.Should().Be(800_000m);
            existingRecord.Status.Should().Be(PayrollRecordStatus.Draft);
        }

        _payrollCalculationService.Received(1).CalculateAsync(
            _employee,
            _workshop,
            _salaryProfiles,
            PeriodStart,
            PeriodEnd,
            Arg.Any<PayrollWorkInput>(),
            Arg.Any<CancellationToken>());

        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLookUpTheExistingRecordByEmployeeAndPeriod()
    {
        SetupPeriod(PeriodStart, PeriodEnd);

        await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        await _payrollRecordRepository.Received(1).GetByEmployeeAndPeriodAsync(
            ValidUserId,
            ValidEmployeeId,
            PeriodStart,
            PeriodEnd,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLoadTheSalaryProfilesOfTheResolvedPeriod()
    {
        SetupPeriod(PeriodStart, PeriodEnd);

        await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreesAffectingPeriodAsync(
            ValidUserId,
            ValidEmployeeId,
            PeriodStart,
            PeriodEnd,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreatePayrollRecordFromTheCalculation()
    {
        var createdId = Guid.NewGuid();
        var work = _payrollRecordBuilder.BuildUserWorkInputDto();
        SetupPeriod(PeriodStart, PeriodEnd);
        _payrollRecordRepository
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(CreateValidCommand(work), CancellationToken.None);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.PayrollRecordId.Should().Be(createdId);
        }

        _payrollCalculationService.Received(1).CalculateAsync(
            _employee,
            _workshop,
            _salaryProfiles,
            PeriodStart,
            PeriodEnd,
            Arg.Is<PayrollWorkInput>(x =>
                x.WorkedDaysCount == work.WorkedDaysCount &&
                x.StandardWorkingDaysCount == PeriodEnd.DayNumber - PeriodStart.DayNumber + 1 &&
                x.IsEsfandPeriod == false),
            Arg.Any<CancellationToken>());

        await _payrollRecordRepository.Received(1).CreateAsync(
            Arg.Is<PayrollRecord>(x =>
                x.EmployeeId == ValidEmployeeId &&
                x.PeriodStart == PeriodStart &&
                x.PeriodEnd == PeriodEnd &&
                x.Status == PayrollRecordStatus.Draft &&
                x.OvertimeHours == 4m &&
                x.OvertimeAmount == 800_000m &&
                x.GrossAmount == 17_900_000m &&
                x.InsuranceAmount == 1_400_000m &&
                x.TotalDeductionsAmount == 2_900_000m &&
                x.NetPayableAmount == 15_000_000m &&
                x.IsEsfandPeriod == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsTheRecord_ShouldReturnGeneralFailure()
    {
        SetupPeriod(PeriodStart, PeriodEnd);
        SetupLimits(new PayrollLimits(2m, 12m, 8m, 8m));

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.ShouldBeFailure(
            "ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.",
            BadResultType.General);
        await _payrollRecordRepository.DidNotReceive()
            .CreateAsync(Arg.Any<PayrollRecord>(), Arg.Any<CancellationToken>());
    }
}
