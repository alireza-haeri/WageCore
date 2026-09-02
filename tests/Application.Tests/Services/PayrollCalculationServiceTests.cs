using Microsoft.Extensions.Logging;

namespace Application.Tests.Services;

public class PayrollCalculationServiceTests
{
    private const decimal DefaultItemAmount = 1_000_000m;

    private static readonly Guid EmployeeId = Guid.NewGuid();
    private static readonly Guid WorkshopId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);

    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly ICalculationFormulaQuery _calculationFormulaQuery;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly CapturingLogger _logger;
    private readonly PayrollCalculationService _service;

    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();
    private readonly Employee _employee;
    private readonly Workshop _workshop;
    private readonly IReadOnlyList<SalaryDecree> _salaryProfiles;

    public PayrollCalculationServiceTests()
    {
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _calculationFormulaQuery = Substitute.For<ICalculationFormulaQuery>();
        _formulaEvaluator = Substitute.For<IFormulaEvaluator>();
        _persianCalendarService = Substitute.For<IPersianCalendarService>();
        _logger = new CapturingLogger();

        _employee = new EmployeeBuilder()
            .WithId(EmployeeId)
            .WithWorkshopId(WorkshopId)
            .CreateResult()
            .ShouldBeSuccess();
        _workshop = new WorkshopBuilder()
            .WithId(WorkshopId)
            .WithUserId(Guid.NewGuid())
            .CreateResult()
            .ShouldBeSuccess();
        _salaryProfiles =
        [
            new SalaryDecreeBuilder()
                .WithEmployeeId(EmployeeId)
                .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
                .WithEffectiveFrom(new DateOnly(2024, 12, 1))
                .CreateResult()
                .ShouldBeSuccess()
        ];

        _service = new PayrollCalculationService(
            _laborLawRuleQuery,
            _calculationFormulaQuery,
            _formulaEvaluator,
            _persianCalendarService,
            _logger);

        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodEnd)
            .Returns(4);
        SetupRules();
        SetupFormulas();
        SetupEvaluation(DefaultItemAmount);
    }

    private void SetupRules()
    {
        SetupRule(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, 80m);
        SetupRule(LaborLawRuleKey.MaximumFridayWorkHoursPerMonth, 12m);
        SetupRule(LaborLawRuleKey.InsurancePercentage, 7m);
        SetupRule(LaborLawRuleKey.AnnualBonusMinimumAmount, 3_000_000m);
        SetupRule(LaborLawRuleKey.AnnualBonusMaximumAmount, 6_000_000m);
        SetupRule(LaborLawRuleKey.TaxExemptMonthlyAmount, 0m);
        SetupRule(LaborLawRuleKey.TaxRatePercentage, 10m);
    }

    private void SetupRule(LaborLawRuleKey key, decimal? value) =>
        _laborLawRuleQuery
            .GetActiveValueAsync(key, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(value);

    private void SetupFormulas()
    {
        foreach (var key in Enum.GetValues<FormulaKey>())
            SetupFormula(key, $"[{key}] * 1");
    }

    private void SetupFormula(FormulaKey key, string? expression) =>
        _calculationFormulaQuery
            .GetActiveExpressionAsync(key, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(expression);

    private void SetupEvaluation(decimal amount) =>
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(DomainResult<decimal>.Success(amount));

    private void SetupEvaluationFailure(string message) =>
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(DomainResult<decimal>.Failure(message));

    private PayrollWorkInputDto BuildWorkInput() => _payrollRecordBuilder.BuildDto();

    private Task<Result<PayrollCalculationResult>> Calculate(
        PayrollWorkInputDto? workInput = null,
        CancellationToken cancellationToken = default) =>
        _service.CalculateAsync(
            _employee,
            _workshop,
            _salaryProfiles,
            PeriodStart,
            PeriodEnd,
            workInput ?? BuildWorkInput(),
            cancellationToken);

    [Fact]
    public async Task CalculateAsync_ShouldEvaluateEveryItemAndReturnTheAmounts()
    {
        var result = await Calculate();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.CalculatedAmounts.BaseSalaryAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.AttractionAllowanceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.SupervisionAllowanceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.NightShiftExtraAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.HolidayWorkAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.ChildAllowanceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.HousingAllowanceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.FoodAllowanceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.MarriageAllowanceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.OvertimeAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.ShiftWorkAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.DailyMissionAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.FridayWorkAllowance.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.EndOfServiceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.AnnualBonusAmount.Should().Be(0m);
            response.CalculatedAmounts.CommutingAllowanceAmount.Should().Be(DefaultItemAmount);
        }
    }

    [Theory]
    [InlineData(FormulaKey.BaseSalaryPay)]
    [InlineData(FormulaKey.AttractionAllowancePay)]
    [InlineData(FormulaKey.SupervisionAllowancePay)]
    [InlineData(FormulaKey.NightShiftExtraPay)]
    [InlineData(FormulaKey.HolidayWorkPay)]
    [InlineData(FormulaKey.ChildAllowancePay)]
    [InlineData(FormulaKey.HousingAllowancePay)]
    [InlineData(FormulaKey.FoodAllowancePay)]
    [InlineData(FormulaKey.MarriageAllowancePay)]
    [InlineData(FormulaKey.OvertimePay)]
    [InlineData(FormulaKey.ShiftWorkPay)]
    [InlineData(FormulaKey.DailyMissionPay)]
    [InlineData(FormulaKey.FridayWorkPay)]
    [InlineData(FormulaKey.EndOfServicePay)]
    [InlineData(FormulaKey.CommutingAllowancePay)]
    public async Task CalculateAsync_ShouldFetchTheFormulaForEachItem(FormulaKey formulaKey)
    {
        await Calculate();

        await _calculationFormulaQuery.Received(1)
            .GetActiveExpressionAsync(formulaKey, PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_ShouldFetchTheRulesForTheItemsThatDependOnLaborLawRules()
    {
        await Calculate();

        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.MaximumFridayWorkHoursPerMonth, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.InsurancePercentage, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxExemptMonthlyAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxRatePercentage, PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassTheRuleValueToTheOvertimeFormula()
    {
        object[]? overtimeFormulaInputs = null;
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                if (ci.Arg<string>() == "[OvertimePay] * 1")
                    overtimeFormulaInputs = ci.Arg<object[]>();

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        await Calculate();

        overtimeFormulaInputs.Should().NotBeNull();
        overtimeFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v =>
                v.Name == nameof(LaborLawRuleKey.MaximumOvertimeHoursPerMonth) &&
                Equals(v.Value, 80m));
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassTheWorkInputToTheFormula()
    {
        var workInput = BuildWorkInput();
        object[]? formulaInputs = null;
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                formulaInputs = ci.Arg<object[]>();

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        await Calculate(workInput);

        formulaInputs.Should().NotBeNull();
        formulaInputs!.Should().Contain(workInput);
    }

    [Fact]
    public async Task CalculateAsync_WithMissionAmountOverride_ShouldUseTheOverrideAndSkipTheDailyMissionFormula()
    {
        var workInput = BuildWorkInput() with { MissionAmountOverride = 500_000m };

        var result = await Calculate(workInput);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.CalculatedAmounts.DailyMissionAmount.Should().Be(500_000m);
            // 14 formula items at 1,000,000 each + the 500,000 override (annual bonus skipped)
            response.Amounts.GrossAmount.Should().Be(14_500_000m);
        }

        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.DailyMissionPay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WithAnnualBonusTypeMinimum_ShouldFetchTheMinimumBonusRule()
    {
        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Minimum
        };

        await Calculate(workInput);

        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMinimumAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMaximumAmount, PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WithAnnualBonusTypeMaximum_ShouldFetchTheMaximumBonusRule()
    {
        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Maximum
        };

        await Calculate(workInput);

        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMaximumAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMinimumAmount, PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WithoutAnnualBonusType_ShouldSkipTheAnnualBonus()
    {
        var result = await Calculate();

        result.ShouldBeSuccess().CalculatedAmounts.AnnualBonusAmount.Should().Be(0m);
        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.AnnualBonusPay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WhenARuleIsMissing_ShouldReturnNotfoundFailureAndLog()
    {
        SetupRule(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, null);

        var result = await Calculate();

        result.ShouldBeFailure(null, BadResultType.NotFound);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("MaximumOvertimeHoursPerMonth"));
        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.OvertimePay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WhenAFormulaIsMissing_ShouldReturnNotfoundFailureAndLog()
    {
        SetupFormula(FormulaKey.BaseSalaryPay, null);

        var result = await Calculate();

        result.ShouldBeFailure(null, BadResultType.NotFound);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("BaseSalaryPay"));
    }

    [Fact]
    public async Task CalculateAsync_WhenTheFormulaEvaluationFails_ShouldReturnGeneralFailureAndLog()
    {
        SetupEvaluationFailure("خطای آزمون");

        var result = await Calculate();

        result.ShouldBeFailure(null, BadResultType.General);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Error && e.Message.Contains("خطای آزمون"));
    }

    [Fact]
    public async Task CalculateAsync_WithACanceledToken_ShouldThrowOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var act = async () => await Calculate(cancellationToken: cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(Arg.Any<LaborLawRuleKey>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(Arg.Any<FormulaKey>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_ShouldComputeTheTotalsFromTheItems()
    {
        var workInput = BuildWorkInput() with
        {
            PerformanceBonusAmount = 500_000m,
            CashBenefitsAmount = 200_000m
        };

        var result = await Calculate(workInput);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            // 15 items at 1,000,000 each + 500,000 performance bonus + 200,000 cash benefits
            response.Amounts.GrossAmount.Should().Be(15_700_000m);
            // 7% insurance rule
            response.Amounts.InsuranceAmount.Should().Be(1_099_000m);
            // 10% tax rule over the whole gross (no exemption)
            response.Amounts.CalculatedTaxAmount.Should().Be(1_570_000m);
            // insurance + tax
            response.Amounts.TotalDeductionsAmount.Should().Be(2_669_000m);
            // gross - total deductions
            response.Amounts.NetPayableAmount.Should().Be(13_031_000m);
        }
    }

    [Fact]
    public async Task CalculateAsync_WithoutOptionalAmounts_ShouldSkipThemAndExcludeThemFromGross()
    {
        var result = await Calculate();

        var response = result.ShouldBeSuccess();
        response.Amounts.GrossAmount.Should().Be(15_000_000m);
    }

    [Fact]
    public async Task CalculateAsync_ShouldUseTheFetchedInsurancePercentageForTheInsuranceAmount()
    {
        SetupRule(LaborLawRuleKey.InsurancePercentage, 8m);

        var result = await Calculate();

        result.ShouldBeSuccess().Amounts.InsuranceAmount.Should().Be(1_200_000m);
    }

    [Fact]
    public async Task CalculateAsync_ShouldUseTheFetchedTaxRulesForTheTaxAmount()
    {
        SetupRule(LaborLawRuleKey.TaxExemptMonthlyAmount, 2_000_000m);
        SetupRule(LaborLawRuleKey.TaxRatePercentage, 20m);

        var result = await Calculate();

        // (15,000,000 - 2,000,000) * 20%
        result.ShouldBeSuccess().Amounts.CalculatedTaxAmount.Should().Be(2_600_000m);
    }

    [Fact]
    public async Task CalculateAsync_WhenGrossIsBelowTheTaxExemption_ShouldCalculateZeroTax()
    {
        SetupRule(LaborLawRuleKey.TaxExemptMonthlyAmount, 100_000_000m);

        var result = await Calculate();

        result.ShouldBeSuccess().Amounts.CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithNullWorkInput_ShouldReturnGeneralFailure()
    {
        var result = await _service.CalculateAsync(
            _employee,
            _workshop,
            _salaryProfiles,
            PeriodStart,
            PeriodEnd,
            null!);

        result.ShouldBeFailure("اطلاعات کارکرد کارمند نمیتواند خالی باشد.", BadResultType.General);
    }

    [Fact]
    public async Task CalculateAsync_WithNoSalaryProfiles_ShouldReturnNotfoundFailure()
    {
        var result = await _service.CalculateAsync(
            _employee,
            _workshop,
            [],
            PeriodStart,
            PeriodEnd,
            BuildWorkInput());

        result.ShouldBeFailure("حکم حقوقی کارمند یافت نشد.", BadResultType.General);
    }

    private sealed class CapturingLogger : ILogger<PayrollCalculationService>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));
    }
}
