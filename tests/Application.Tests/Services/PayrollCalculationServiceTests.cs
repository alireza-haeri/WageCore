using Microsoft.Extensions.Logging;

namespace Application.Tests.Services;

public class PayrollCalculationServiceTests
{
    private const decimal DefaultItemAmount = 1_000_000m;

    private static readonly Guid EmployeeId = Guid.NewGuid();
    private static readonly Guid WorkshopId = Guid.NewGuid();
    private static readonly DateOnly PeriodStart = new(2025, 1, 1);
    private static readonly DateOnly PeriodEnd = new(2025, 1, 31);

    private static readonly FormulaEvaluator RealEvaluator =
        new(Substitute.For<ILogger<FormulaEvaluator>>());

    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly ICalculationFormulaQuery _calculationFormulaQuery;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IPersianCalendarService _persianCalendarService;
    private readonly IPayrollRecordQuery _payrollRecordQuery;
    private readonly CapturingLogger _logger;
    private readonly PayrollCalculationService _service;

    private readonly Guid _workshopUserId = Guid.NewGuid();
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
        _payrollRecordQuery = Substitute.For<IPayrollRecordQuery>();
        _logger = new CapturingLogger();

        _employee = new EmployeeBuilder()
            .WithId(EmployeeId)
            .WithWorkshopId(WorkshopId)
            .CreateResult()
            .ShouldBeSuccess();
        _workshop = new WorkshopBuilder()
            .WithId(WorkshopId)
            .WithUserId(_workshopUserId)
            .CreateResult()
            .ShouldBeSuccess();
        _salaryDecrees =
        [
            new SalaryDecreeBuilder()
                .WithEmployeeId(EmployeeId)
                .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
                .WithEffectiveFrom(new DateOnly(2024, 12, 1))
                .WithShiftType(ShiftType.MorningEvening)
                .CreateResult()
                .ShouldBeSuccess()
        ];

        _service = new PayrollCalculationService(
            _laborLawRuleQuery,
            _calculationFormulaQuery,
            _formulaEvaluator,
            _persianCalendarService,
            _payrollRecordQuery,
            _logger);

        _persianCalendarService
            .GetFridayCount(PeriodStart, PeriodEnd)
            .Returns(4);
        _persianCalendarService
            .GetPersianMonth(Arg.Any<DateOnly>())
            .Returns(1);
        _persianCalendarService
            .GetDaysInPersianYear(Arg.Any<DateOnly>())
            .Returns(365);
        _payrollRecordQuery
            .GetAnnualWorkedDaysCountAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(0m);
        SetupRules();
        SetupFormulas();
        SetupEvaluation(DefaultItemAmount);
    }

    private void SetupRules()
    {
        _laborLawRuleQuery
            .GetActiveRuleValuesAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyDictionary<LaborLawRuleKey, decimal>>(
                new Dictionary<LaborLawRuleKey, decimal>(PayrollFormulaCatalog.RuleValues));
    }

    private void SetupRule(LaborLawRuleKey key, decimal? value)
    {
        var values = new Dictionary<LaborLawRuleKey, decimal>(PayrollFormulaCatalog.RuleValues);
        if (value is null)
            values.Remove(key);
        else
            values[key] = value.Value;

        _laborLawRuleQuery
            .GetActiveRuleValuesAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyDictionary<LaborLawRuleKey, decimal>>(values);
    }

    private void SetupFormulas()
    {
        foreach (var key in Enum.GetValues<FormulaKey>())
            SetupFormula(key, PayrollFormulaCatalog.Expression(key));
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

    private IReadOnlyList<(string Expression, object[] Inputs)> CaptureEvaluationCalls()
    {
        var calls = new List<(string Expression, object[] Inputs)>();
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                calls.Add((ci.Arg<string>(), ci.Arg<object[]>()));
                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        return calls;
    }

    private static decimal EvaluateWithRealEvaluator(string expression, object[] inputs) =>
        RealEvaluator.Evaluate(expression, inputs).ShouldBeSuccess();

    private PayrollWorkInput BuildWorkInput() => _payrollRecordBuilder.BuildPayrollWorkInput();

    private Task<Result<PayrollCalculationResult>> Calculate(
        PayrollWorkInput? workInput = null,
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
    [InlineData(FormulaKey.TaxableAmountPay)]
    [InlineData(FormulaKey.TaxPay)]
    public async Task CalculateAsync_ShouldFetchTheFormulaForEachItem(FormulaKey formulaKey)
    {
        await Calculate();

        await _calculationFormulaQuery.Received(1)
            .GetActiveExpressionAsync(formulaKey, PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_ShouldFetchAllRuleValuesInASingleQuery()
    {
        await Calculate();

        await _laborLawRuleQuery.Received(1)
            .GetActiveRuleValuesAsync(PeriodStart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassThePercentageAndStandardHoursRulesToTheOvertimeFormula()
    {
        object[]? overtimeFormulaInputs = null;
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                if (ci.Arg<string>() == PayrollFormulaCatalog.Expression(FormulaKey.OvertimePay))
                    overtimeFormulaInputs = ci.Arg<object[]>();

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        await Calculate();

        overtimeFormulaInputs.Should().NotBeNull();
        overtimeFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v =>
                v.Name == nameof(LaborLawRuleKey.OvertimePercentage) &&
                Equals(v.Value, PayrollFormulaCatalog.OvertimePercentage));
        overtimeFormulaInputs!
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v =>
                v.Name == nameof(LaborLawRuleKey.StandardDailyWorkHours) &&
                Equals(v.Value, PayrollFormulaCatalog.StandardDailyWorkHoursValue));
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
            response.Amounts.GrossAmount.Should().Be(14_500_000m);
        }

        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.DailyMissionPay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WithAnnualBonusTypeMinimum_ShouldPassMinimumBonusAmountToTheAnnualBonusFormula()
    {
        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Minimum
        };

        var calls = CaptureEvaluationCalls();

        var result = await Calculate(workInput);

        result.ShouldBeSuccess().CalculatedAmounts.AnnualBonusAmount.Should().Be(DefaultItemAmount);

        var annualBonusInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.AnnualBonusPay)).Inputs;
        annualBonusInputs
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v =>
                v.Name == "AnnualBonusRuleAmount" &&
                Equals(v.Value, PayrollFormulaCatalog.AnnualBonusMinimumAmount));
    }

    [Fact]
    public async Task CalculateAsync_WithAnnualBonusTypeMaximum_ShouldPassMaximumBonusAmountToTheAnnualBonusFormula()
    {
        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Maximum
        };

        var calls = CaptureEvaluationCalls();

        await Calculate(workInput);

        var annualBonusInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.AnnualBonusPay)).Inputs;
        annualBonusInputs
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v =>
                v.Name == "AnnualBonusRuleAmount" &&
                Equals(v.Value, PayrollFormulaCatalog.AnnualBonusMaximumAmount));
    }

    [Fact]
    public async Task CalculateAsync_WithoutAnnualBonusType_ShouldSkipTheAnnualBonusAndReturnNull()
    {
        var result = await Calculate();

        result.ShouldBeSuccess().CalculatedAmounts.AnnualBonusAmount.Should().BeNull();
        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.AnnualBonusPay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(ShiftType.MorningEvening, LaborLawRuleKey.ShiftWorkPercentageMorningEvening)]
    [InlineData(ShiftType.MorningNight, LaborLawRuleKey.ShiftWorkPercentageMorningNight)]
    [InlineData(ShiftType.EveningNight, LaborLawRuleKey.ShiftWorkPercentageEveningNight)]
    [InlineData(ShiftType.MorningEveningNight, LaborLawRuleKey.ShiftWorkPercentageMorningEveningNight)]
    public async Task CalculateAsync_WithEachShiftType_ShouldPassItsOwnShiftWorkRuleAsShiftWorkPercentage(
        ShiftType shiftType, LaborLawRuleKey expectedRuleKey)
    {
        IReadOnlyList<SalaryDecree> salaryDecrees =
        [
            new SalaryDecreeBuilder()
                .WithEmployeeId(EmployeeId)
                .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
                .WithEffectiveFrom(new DateOnly(2024, 12, 1))
                .WithShiftType(shiftType)
                .CreateResult()
                .ShouldBeSuccess()
        ];

        var calls = CaptureEvaluationCalls();

        var result = await Calculate(salaryDecrees: salaryDecrees);

        result.ShouldBeSuccess().CalculatedAmounts.ShiftWorkAmount.Should().Be(DefaultItemAmount);

        var shiftWorkInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.ShiftWorkPay)).Inputs;
        shiftWorkInputs
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v =>
                v.Name == "ShiftWorkPercentage" &&
                Equals(v.Value, PayrollFormulaCatalog.RuleValues[expectedRuleKey]));
    }

    [Fact]
    public async Task CalculateAsync_WithShiftTypeNone_ShouldSkipTheShiftWorkPayItemAndLog()
    {
        IReadOnlyList<SalaryDecree> salaryDecrees =
        [
            new SalaryDecreeBuilder()
                .WithEmployeeId(EmployeeId)
                .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
                .WithEffectiveFrom(new DateOnly(2024, 12, 1))
                .WithShiftType(ShiftType.None)
                .CreateResult()
                .ShouldBeSuccess()
        ];

        var result = await Calculate(salaryDecrees: salaryDecrees);

        result.ShouldBeSuccess().CalculatedAmounts.ShiftWorkAmount.Should().Be(0m);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information && e.Message.Contains("Shift type is None"));
        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.ShiftWorkPay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WhenAnItemRuleIsMissing_ShouldReturnNotfoundFailureAndLog()
    {
        SetupRule(LaborLawRuleKey.OvertimePercentage, null);

        var result = await Calculate();

        result.ShouldBeFailure(null, BadResultType.NotFound);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("OvertimePercentage"));
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
    public async Task CalculateAsync_WhenTheTaxableAmountFormulaIsMissing_ShouldReturnNotfoundFailureAndLog()
    {
        SetupFormula(FormulaKey.TaxableAmountPay, null);

        var result = await Calculate();

        result.ShouldBeFailure(null, BadResultType.NotFound);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("TaxableAmountPay"));
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
            response.Amounts.GrossAmount.Should().Be(15_700_000m);
            response.Amounts.InsuranceAmount.Should().Be(1_000_000m);
            response.Amounts.CalculatedTaxAmount.Should().Be(1_000_000m);
            response.Amounts.TotalDeductionsAmount.Should().Be(2_000_000m);
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
    public async Task CalculateAsync_ShouldPassAllItemAmountsAndAllRulesToTheInsuranceFormula()
    {
        var workInput = BuildWorkInput() with
        {
            PerformanceBonusAmount = 500_000m,
            CashBenefitsAmount = 200_000m
        };

        var calls = CaptureEvaluationCalls();

        await Calculate(workInput);

        var insuranceInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.InsurancePay)).Inputs;
        var variables = insuranceInputs.OfType<FormulaVariable>().ToList();

        variables.Should().Contain(v => v.Name == nameof(FormulaKey.BaseSalaryPay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.OvertimePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == "PerformanceBonusAmount" && Equals(v.Value, 500_000m));
        variables.Should().Contain(v => v.Name == "CashBenefitsAmount" && Equals(v.Value, 200_000m));
        variables.Should().Contain(v =>
            v.Name == nameof(LaborLawRuleKey.InsurancePercentage) &&
            Equals(v.Value, PayrollFormulaCatalog.InsurancePercentage));

        variables.Should().NotContain(v => v.Name == "GrossAmount");
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassAllItemAmountsAndAllRulesToTheTaxableAmountFormula()
    {
        var workInput = BuildWorkInput() with
        {
            PerformanceBonusAmount = 500_000m,
            CashBenefitsAmount = 200_000m
        };

        var calls = CaptureEvaluationCalls();

        await Calculate(workInput);

        var taxableInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.TaxableAmountPay)).Inputs;
        var variables = taxableInputs.OfType<FormulaVariable>().ToList();

        variables.Should().Contain(v => v.Name == nameof(FormulaKey.BaseSalaryPay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.AttractionAllowancePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.SupervisionAllowancePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.NightShiftExtraPay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.HolidayWorkPay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.ChildAllowancePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.HousingAllowancePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.FoodAllowancePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.MarriageAllowancePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.OvertimePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.ShiftWorkPay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.FridayWorkPay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.CommutingAllowancePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.DailyMissionPay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.EndOfServicePay) && Equals(v.Value, 1_000_000m));
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.AnnualBonusPay) && Equals(v.Value, 0m));
        variables.Should().Contain(v => "PerformanceBonusAmount" == v.Name && Equals(v.Value, 500_000m));
        variables.Should().Contain(v => "CashBenefitsAmount" == v.Name && Equals(v.Value, 200_000m));
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassTheTaxableAmountAndAllRulesToTheTaxFormula()
    {
        var calls = CaptureEvaluationCalls();

        await Calculate();

        var taxInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.TaxPay)).Inputs;
        var variables = taxInputs.OfType<FormulaVariable>().ToList();

        variables.Should().ContainSingle(v => v.Name == "TaxableAmount" && Equals(v.Value, DefaultItemAmount));
        variables.Should().NotContain(v => v.Name == "GrossAmount");
    }

    [Fact]
    public async Task CalculateAsync_WhenTheTaxFormulaReturnsZero_ShouldReportZeroTax()
    {
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
                ci.Arg<string>() == PayrollFormulaCatalog.Expression(FormulaKey.TaxPay)
                    ? DomainResult<decimal>.Success(0m)
                    : DomainResult<decimal>.Success(DefaultItemAmount));

        var result = await Calculate();

        result.ShouldBeSuccess().Amounts.CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_ShouldAddTheCurrentPeriodWorkedDaysToThePersistedAnnualAggregate()
    {
        _payrollRecordQuery
            .GetAnnualWorkedDaysCountAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(100m);

        var calls = CaptureEvaluationCalls();

        await Calculate();

        await _payrollRecordQuery.Received(1)
            .GetAnnualWorkedDaysCountAsync(_workshopUserId, EmployeeId, PeriodStart, Arg.Any<CancellationToken>());
        _persianCalendarService.Received(1).GetDaysInPersianYear(PeriodStart);

        var endOfServiceInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.EndOfServicePay)).Inputs;
        endOfServiceInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 124m));
        endOfServiceInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "DaysInYear" && Equals(v.Value, 365));

        var baseSalaryInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.BaseSalaryPay)).Inputs;
        baseSalaryInputs.OfType<FormulaVariable>()
            .Should()
            .NotContain(v => v.Name == "AnnualWorkedDaysCount");
        baseSalaryInputs.OfType<FormulaVariable>()
            .Should()
            .NotContain(v => v.Name == "DaysInYear");
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassTheAnnualContextToTheAnnualBonusFormulaToo()
    {
        _payrollRecordQuery
            .GetAnnualWorkedDaysCountAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(100m);

        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Maximum
        };

        var calls = CaptureEvaluationCalls();

        await Calculate(workInput);

        var annualBonusInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.AnnualBonusPay)).Inputs;
        annualBonusInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 124m));
        annualBonusInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "DaysInYear" && Equals(v.Value, 365));
        annualBonusInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v =>
                v.Name == "AnnualBonusRuleAmount" &&
                Equals(v.Value, PayrollFormulaCatalog.AnnualBonusMaximumAmount));
    }

    [Fact]
    public async Task CalculateAsync_ShouldAddThePreCurrentMonthWorkedDaysToTheAnnualCountsInTheHireYear()
    {
        // Hired in Farvardin 1403 — the same Persian year as the calculation
        // period (2025-01-01 is Mehr 1403) — with 100 worked days before the
        // current month, unknown to any payroll record.
        var employee = new EmployeeBuilder()
            .WithId(EmployeeId)
            .WithWorkshopId(WorkshopId)
            .WithWorkshopRegistrationDate(new DateOnly(2024, 3, 20))
            .WithHireDate(new DateOnly(2024, 4, 15))
            .WithNetWorkedDaysBeforeCurrentMonth(100)
            .CreateResult()
            .ShouldBeSuccess();

        _persianCalendarService.GetPersianYear(PeriodStart).Returns(1403);
        _persianCalendarService.GetPersianYear(new DateOnly(2024, 4, 15)).Returns(1403);

        _payrollRecordQuery
            .GetAnnualWorkedDaysCountAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(100m);

        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Maximum
        };

        var calls = CaptureEvaluationCalls();

        var result = await _service.CalculateAsync(
            employee, _workshop, _salaryDecrees, PeriodStart, PeriodEnd, workInput);

        result.ShouldBeSuccess();

        var endOfServiceInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.EndOfServicePay)).Inputs;
        // 100 persisted + 24 current + 100 pre-current-month hire-year days.
        endOfServiceInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 224m));

        var annualBonusInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.AnnualBonusPay)).Inputs;
        // The annual bonus is prorated the same way in the hire year.
        annualBonusInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 224m));
    }

    [Fact]
    public async Task CalculateAsync_ShouldNotAddThePreCurrentMonthWorkedDaysOutsideTheHireYear()
    {
        // Hired in 1402 — the calculation year (1403) is a full year covered
        // by payroll records, so the onboarding field must not be added.
        var employee = new EmployeeBuilder()
            .WithId(EmployeeId)
            .WithWorkshopId(WorkshopId)
            .WithWorkshopRegistrationDate(new DateOnly(2023, 3, 20))
            .WithHireDate(new DateOnly(2023, 4, 15))
            .WithNetWorkedDaysBeforeCurrentMonth(100)
            .CreateResult()
            .ShouldBeSuccess();

        _persianCalendarService.GetPersianYear(PeriodStart).Returns(1403);
        _persianCalendarService.GetPersianYear(new DateOnly(2023, 4, 15)).Returns(1402);

        _payrollRecordQuery
            .GetAnnualWorkedDaysCountAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(100m);

        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Maximum
        };

        var calls = CaptureEvaluationCalls();

        var result = await _service.CalculateAsync(
            employee, _workshop, _salaryDecrees, PeriodStart, PeriodEnd, workInput);

        result.ShouldBeSuccess();

        var endOfServiceInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.EndOfServicePay)).Inputs;
        endOfServiceInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 124m));

        var annualBonusInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.AnnualBonusPay)).Inputs;
        annualBonusInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 124m));
    }

    [Fact]
    public async Task CalculateAsync_ShouldTreatAMissingPreCurrentMonthWorkedDaysAsZeroInTheHireYear()
    {
        var employee = new EmployeeBuilder()
            .WithId(EmployeeId)
            .WithWorkshopId(WorkshopId)
            .WithWorkshopRegistrationDate(new DateOnly(2024, 3, 20))
            .WithHireDate(new DateOnly(2024, 4, 15))
            .CreateResult()
            .ShouldBeSuccess();

        _persianCalendarService.GetPersianYear(PeriodStart).Returns(1403);
        _persianCalendarService.GetPersianYear(new DateOnly(2024, 4, 15)).Returns(1403);

        _payrollRecordQuery
            .GetAnnualWorkedDaysCountAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(100m);

        var calls = CaptureEvaluationCalls();

        var result = await _service.CalculateAsync(
            employee, _workshop, _salaryDecrees, PeriodStart, PeriodEnd, BuildWorkInput());

        result.ShouldBeSuccess();

        var endOfServiceInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.EndOfServicePay)).Inputs;
        endOfServiceInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 124m));
    }

    [Fact]
    public async Task CalculateAsync_WhenTheDecreeAllowancesAreNull_ShouldEvaluateTheAllowanceFormulasToZero()
    {
        var calls = CaptureEvaluationCalls();

        await Calculate();

        var attractionInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.AttractionAllowancePay)).Inputs;
        var supervisionInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.SupervisionAllowancePay)).Inputs;
        EvaluateWithRealEvaluator(PayrollFormulaCatalog.Expression(FormulaKey.AttractionAllowancePay), attractionInputs)
            .Should().Be(0m);
        EvaluateWithRealEvaluator(PayrollFormulaCatalog.Expression(FormulaKey.SupervisionAllowancePay), supervisionInputs)
            .Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithRealFormulaEvaluation_ShouldComputeTheTaxableBaseAndTheProgressiveTax()
    {
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                var expression = ci.Arg<string>();
                var inputs = ci.Arg<object[]>();

                return expression == PayrollFormulaCatalog.Expression(FormulaKey.TaxableAmountPay) ||
                       expression == PayrollFormulaCatalog.Expression(FormulaKey.TaxPay)
                    ? DomainResult<decimal>.Success(EvaluateWithRealEvaluator(expression, inputs))
                    : DomainResult<decimal>.Success(DefaultItemAmount);
            });

        var workInput = BuildWorkInput() with
        {
            PerformanceBonusAmount = 500_000m,
            CashBenefitsAmount = 200_000m
        };

        var result = await Calculate(workInput);

        var response = result.ShouldBeSuccess();
        response.Amounts.CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WithRealFormulaEvaluation_ShouldExcludeTheMissionFromTheInsuranceBase()
    {
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                var expression = ci.Arg<string>();
                var inputs = ci.Arg<object[]>();

                return expression == PayrollFormulaCatalog.Expression(FormulaKey.InsurancePay)
                    ? DomainResult<decimal>.Success(EvaluateWithRealEvaluator(expression, inputs))
                    : DomainResult<decimal>.Success(DefaultItemAmount);
            });

        var result = await Calculate();

        var response = result.ShouldBeSuccess();

        // The mission (DailyMissionPay = 1,000,000) must not be part of the
        // insurance base: 12 insurable items at 1,000,000 each * 7%.
        response.Amounts.InsuranceAmount.Should().Be(12_000_000m * PayrollFormulaCatalog.InsurancePercentage / 100);
    }

    [Fact]
    public async Task CalculateAsync_WhenTaxFormulaExpressionChanges_ShouldChangeTheTaxWithoutAnyCodeChange()
    {
        var newTaxFormula = "[TaxableAmount] * 10 / 100";

        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                var expression = ci.Arg<string>();
                var inputs = ci.Arg<object[]>();

                return expression == PayrollFormulaCatalog.Expression(FormulaKey.TaxableAmountPay) ||
                       expression == newTaxFormula
                    ? DomainResult<decimal>.Success(EvaluateWithRealEvaluator(expression, inputs))
                    : DomainResult<decimal>.Success(DefaultItemAmount);
            });

        SetupFormula(FormulaKey.TaxPay, newTaxFormula);

        var result = await Calculate();

        var response = result.ShouldBeSuccess();
        response.Amounts.CalculatedTaxAmount.Should().Be(1_300_000m);
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
    public async Task CalculateAsync_ShouldPassTheWorkInputToTheFormula()
    {
        var workInput = BuildWorkInput();
        var formulaInputsCalls = new List<object[]>();
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                formulaInputsCalls.Add(ci.Arg<object[]>());

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        await Calculate(workInput);

        formulaInputsCalls.Should().NotBeEmpty();
        formulaInputsCalls.First().Should().Contain(workInput);
    }

    [Fact]
    public async Task CalculateAsync_ShouldSelectTheLatestDecreeEffectiveByPeriodEnd()
    {
        var olderDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
            .WithEffectiveFrom(new DateOnly(2024, 11, 1))
            .WithShiftType(ShiftType.MorningEvening)
            .CreateResult()
            .ShouldBeSuccess();
        var midPeriodDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
            .WithEffectiveFrom(new DateOnly(2025, 1, 10))
            .WithShiftType(ShiftType.MorningEvening)
            .CreateResult()
            .ShouldBeSuccess();
        IReadOnlyList<SalaryDecree> salaryDecrees = [olderDecree, midPeriodDecree];

        var formulaInputsCalls = new List<object[]>();
        _formulaEvaluator
            .Evaluate(Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(ci =>
            {
                formulaInputsCalls.Add(ci.Arg<object[]>());

                return DomainResult<decimal>.Success(DefaultItemAmount);
            });

        var result = await Calculate(salaryDecrees: salaryDecrees);

        result.ShouldBeSuccess();
        formulaInputsCalls.Should().NotBeEmpty();
        formulaInputsCalls.First().Should().Contain(midPeriodDecree);
        formulaInputsCalls.SelectMany(inputs => inputs).Should().NotContain(olderDecree);
    }

    [Fact]
    public async Task CalculateAsync_WhenNoDecreeIsEffectiveByPeriodEnd_ShouldReturnNotfoundFailureAndLog()
    {
        var futureDecree = new SalaryDecreeBuilder()
            .WithEmployeeId(EmployeeId)
            .WithEmployeeHireDate(new DateOnly(2024, 6, 1))
            .WithEffectiveFrom(new DateOnly(2025, 2, 1))
            .WithShiftType(ShiftType.MorningEvening)
            .CreateResult()
            .ShouldBeSuccess();
        IReadOnlyList<SalaryDecree> salaryDecrees = [futureDecree];

        var result = await Calculate(salaryDecrees: salaryDecrees);

        result.ShouldBeFailure("حکم حقوقی فعال برای این کارمند در این بازه یافت نشد.", BadResultType.NotFound);
        _logger.Entries.Should().Contain(e => e.Level == LogLevel.Warning);
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