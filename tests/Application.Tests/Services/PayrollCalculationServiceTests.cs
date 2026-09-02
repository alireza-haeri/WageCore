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
    private readonly IReadOnlyList<SalaryDecree> _salaryDecrees;

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
        _salaryDecrees =
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
        _persianCalendarService
            .GetPersianMonth(Arg.Any<DateOnly>())
            .Returns(1);
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
        CancellationToken cancellationToken = default,
        IReadOnlyList<SalaryDecree>? salaryDecrees = null) =>
        _service.CalculateAsync(
            _employee,
            _workshop,
            salaryDecrees ?? _salaryDecrees,
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
            response.CalculatedAmounts.AnnualBonusAmount.Should().BeNull();
            response.CalculatedAmounts.CommutingAllowanceAmount.Should().Be(DefaultItemAmount);
            response.CalculatedAmounts.PerformanceBonusAmount.Should().BeNull();
            response.CalculatedAmounts.CashBenefitsAmount.Should().BeNull();
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
    [InlineData(FormulaKey.InsurancePay)]
    [InlineData(FormulaKey.TaxPay)]
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

        var result = await Calculate(workInput);

        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMinimumAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMaximumAmount, PeriodStart, Arg.Any<CancellationToken>());
        result.ShouldBeSuccess().CalculatedAmounts.AnnualBonusAmount.Should().Be(DefaultItemAmount);
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
    public async Task CalculateAsync_WithoutAnnualBonusType_ShouldSkipTheAnnualBonusAndReturnNull()
    {
        var result = await Calculate();

        result.ShouldBeSuccess().CalculatedAmounts.AnnualBonusAmount.Should().BeNull();
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
            // insurance and tax are formula-driven too; the evaluator stub returns 1,000,000 for each
            response.Amounts.InsuranceAmount.Should().Be(1_000_000m);
            response.Amounts.CalculatedTaxAmount.Should().Be(1_000_000m);
            // insurance + tax
            response.Amounts.TotalDeductionsAmount.Should().Be(2_000_000m);
            // gross - total deductions
            response.Amounts.NetPayableAmount.Should().Be(13_700_000m);
            response.CalculatedAmounts.PerformanceBonusAmount.Should().Be(500_000m);
            response.CalculatedAmounts.CashBenefitsAmount.Should().Be(200_000m);
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
    public async Task CalculateAsync_ShouldPassTheGrossAndInsurancePercentageToTheInsuranceFormula()
    {
        object[]? insuranceFormulaInputs = null;
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                if (ci.Arg<string>() == "[InsurancePay] * 1")
                    insuranceFormulaInputs = ci.Arg<object[]>();

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        await Calculate();

        insuranceFormulaInputs.Should().NotBeNull();
        insuranceFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .Contain(v => v.Name == "GrossAmount" && Equals(v.Value, 15_000_000m));
        insuranceFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .Contain(v =>
                v.Name == nameof(LaborLawRuleKey.InsurancePercentage) &&
                Equals(v.Value, 7m));
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassTheGrossAndTaxRulesToTheTaxFormula()
    {
        SetupRule(LaborLawRuleKey.TaxExemptMonthlyAmount, 2_000_000m);
        SetupRule(LaborLawRuleKey.TaxRatePercentage, 20m);

        object[]? taxFormulaInputs = null;
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                if (ci.Arg<string>() == "[TaxPay] * 1")
                    taxFormulaInputs = ci.Arg<object[]>();

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        await Calculate();

        taxFormulaInputs.Should().NotBeNull();
        taxFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .Contain(v => v.Name == "GrossAmount" && Equals(v.Value, 15_000_000m));
        taxFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .Contain(v =>
                v.Name == nameof(LaborLawRuleKey.TaxExemptMonthlyAmount) &&
                Equals(v.Value, 2_000_000m));
        taxFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .Contain(v =>
                v.Name == nameof(LaborLawRuleKey.TaxRatePercentage) &&
                Equals(v.Value, 20m));
    }

    [Fact]
    public async Task CalculateAsync_WhenTheTaxFormulaReturnsZero_ShouldReportZeroTax()
    {
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
                ci.Arg<string>() == "[TaxPay] * 1"
                    ? DomainResult<decimal>.Success(0m)
                    : DomainResult<decimal>.Success(DefaultItemAmount));

        var result = await Calculate();

        result.ShouldBeSuccess().Amounts.CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithNullWorkInput_ShouldReturnGeneralFailure()
    {
        var result = await _service.CalculateAsync(
            _employee,
            _workshop,
            _salaryDecrees,
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

        result.ShouldBeFailure("برای این بازه حکم حقوقی کارمند یافت نشد.", BadResultType.NotFound);
    }

    [Fact]
    public async Task CalculateAsync_ShouldSelectTheLatestDecreeEffectiveByPeriodEnd()
    {
        var olderDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
            .WithEffectiveFrom(new DateOnly(2024, 11, 1))
            .CreateResult()
            .ShouldBeSuccess();
        var midPeriodDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
            .WithEffectiveFrom(new DateOnly(2025, 1, 10))
            .CreateResult()
            .ShouldBeSuccess();
        IReadOnlyList<SalaryDecree> salaryDecrees = [olderDecree, midPeriodDecree];

        object[]? formulaInputs = null;
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                formulaInputs = ci.Arg<object[]>();

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        var result = await Calculate(salaryDecrees: salaryDecrees);

        result.ShouldBeSuccess();
        formulaInputs.Should().NotBeNull();
        formulaInputs!.Should().Contain(midPeriodDecree);
        formulaInputs!.Should().NotContain(olderDecree);
    }

    [Fact]
    public async Task CalculateAsync_WhenNoDecreeIsEffectiveByPeriodEnd_ShouldReturnNotfoundFailureAndLog()
    {
        var futureDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
            .WithEffectiveFrom(new DateOnly(2025, 2, 1))
            .CreateResult()
            .ShouldBeSuccess();
        IReadOnlyList<SalaryDecree> salaryDecrees = [futureDecree];

        var result = await Calculate(salaryDecrees: salaryDecrees);

        result.ShouldBeFailure("حکم حقوقی فعال برای این کارمند در این بازه یافت نشد.", BadResultType.NotFound);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task CalculateAsync_WhenThePeriodIsInEsfand_ShouldReportIsEsfandPeriod()
    {
        _persianCalendarService
            .GetPersianMonth(PeriodStart)
            .Returns(12);

        var result = await Calculate();

        result.ShouldBeSuccess().IsEsfandPeriod.Should().BeTrue();
        _persianCalendarService.Received(1).GetPersianMonth(PeriodStart);
    }

    [Fact]
    public async Task CalculateAsync_WhenThePeriodIsOutsideEsfand_ShouldReportIsEsfandPeriodFalse()
    {
        _persianCalendarService
            .GetPersianMonth(PeriodStart)
            .Returns(1);

        var result = await Calculate();

        result.ShouldBeSuccess().IsEsfandPeriod.Should().BeFalse();
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
