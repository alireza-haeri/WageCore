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
        foreach (var (key, value) in PayrollFormulaCatalog.RuleValues)
            SetupRule(key, value);
    }

    private void SetupRule(LaborLawRuleKey key, decimal? value) =>
        _laborLawRuleQuery
            .GetActiveValueAsync(key, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(value);

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

    // Captures every formula evaluation (expression + inputs) while the evaluator
    // stub keeps returning the default amount.
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
    [InlineData(FormulaKey.TaxableAmountPay)]
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
            .GetActiveValueAsync(LaborLawRuleKey.NightShiftPercentage, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.HolidayWorkPercentage, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.ChildAllowanceMultiplier, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.HousingAllowanceAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.FoodAllowanceAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.MarriageAllowanceAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.OvertimePercentage, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.FridayWorkPercentage, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.EndOfServiceDaysPerYear, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.MinimumDailySalary, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(4)
            .GetActiveValueAsync(LaborLawRuleKey.StandardDailyWorkHours, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.ShiftWorkPercentageMorningEvening, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.InsurancePercentage, PeriodStart, Arg.Any<CancellationToken>());

        // Every tax bracket threshold/rate is fetched as a rule.
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket1Threshold, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket2Threshold, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket2Rate, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket3Threshold, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket3Rate, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket4Threshold, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket4Rate, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket5Threshold, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket5Rate, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(LaborLawRuleKey.TaxBracket6Rate, PeriodStart, Arg.Any<CancellationToken>());

        // The cap/limit rules belong to the payroll limits resolver, not to the
        // item formulas; the old single-rate tax rules are deprecated.
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.MaximumOvertimeHoursPerMonth, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.MaximumFridayWorkHoursPerMonth, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.TaxExemptMonthlyAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.TaxRatePercentage, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMinimumAmount, PeriodStart, Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.AnnualBonusMaximumAmount, PeriodStart, Arg.Any<CancellationToken>());
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
    public async Task CalculateAsync_WithAnnualBonusType_ShouldPassTheResolvedRuleUnderTheFixedVariableName()
    {
        var workInput = BuildWorkInput() with
        {
            IsEsfandPeriod = true,
            AnnualBonusType = AnnualBonusType.Minimum
        };

        var calls = CaptureEvaluationCalls();

        await Calculate(workInput);

        var annualBonusInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.AnnualBonusPay)).Inputs;
        annualBonusInputs
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualBonusRuleAmount" && Equals(v.Value, PayrollFormulaCatalog.AnnualBonusMinimumAmount));
        annualBonusInputs.OfType<FormulaVariable>().Should().NotContain(v => v.Name == nameof(LaborLawRuleKey.AnnualBonusMinimumAmount));
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
    public async Task CalculateAsync_WithEachShiftType_ShouldFetchItsOwnShiftWorkRuleAndPassItAsShiftWorkPercentage(
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
        await _laborLawRuleQuery.Received(1)
            .GetActiveValueAsync(expectedRuleKey, PeriodStart, Arg.Any<CancellationToken>());

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
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.ShiftWorkPercentageMorningEvening, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.ShiftWorkPercentageMorningNight, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.ShiftWorkPercentageEveningNight, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
        await _laborLawRuleQuery.DidNotReceive()
            .GetActiveValueAsync(LaborLawRuleKey.ShiftWorkPercentageMorningEveningNight, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WhenARuleIsMissing_ShouldReturnNotfoundFailureAndLog()
    {
        SetupRule(LaborLawRuleKey.OvertimePercentage, null);

        var result = await Calculate();

        result.ShouldBeFailure(null, BadResultType.NotFound);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("OvertimePercentage"));
        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.OvertimePay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WhenATaxBracketRuleIsMissing_ShouldReturnNotfoundFailureAndLog()
    {
        SetupRule(LaborLawRuleKey.TaxBracket3Rate, null);

        var result = await Calculate();

        result.ShouldBeFailure(null, BadResultType.NotFound);
        _logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("TaxBracket3Rate"));
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
        await _payrollRecordQuery.DidNotReceive()
            .GetAnnualWorkedDaysCountAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
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
                if (ci.Arg<string>() == PayrollFormulaCatalog.Expression(FormulaKey.InsurancePay))
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
                Equals(v.Value, PayrollFormulaCatalog.InsurancePercentage));
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassEveryItemAmountToTheTaxableAmountFormulaExceptMissionAndEndOfService()
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

        // One variable per taxable item (all items but mission and end-of-service)
        // plus the optional performance/cash amounts.
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
        // Annual bonus is not applicable in this period, so its variable is zero.
        variables.Should().Contain(v => v.Name == nameof(FormulaKey.AnnualBonusPay) && Equals(v.Value, 0m));
        variables.Should().Contain(v => v.Name == "PerformanceBonusAmount" && Equals(v.Value, 500_000m));
        variables.Should().Contain(v => v.Name == "CashBenefitsAmount" && Equals(v.Value, 200_000m));

        variables.Should().NotContain(v => v.Name == nameof(FormulaKey.DailyMissionPay));
        variables.Should().NotContain(v => v.Name == nameof(FormulaKey.EndOfServicePay));
    }

    [Fact]
    public async Task CalculateAsync_ShouldPassTheTaxableAmountAndEveryBracketRuleToTheTaxFormula()
    {
        var calls = CaptureEvaluationCalls();

        await Calculate();

        var taxInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.TaxPay)).Inputs;
        taxInputs
            .OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "TaxableAmount" && Equals(v.Value, DefaultItemAmount));

        taxInputs.OfType<FormulaVariable>()
            .Should()
            .Contain(v =>
                v.Name == nameof(LaborLawRuleKey.TaxBracket1Threshold) &&
                Equals(v.Value, PayrollFormulaCatalog.TaxBracket1Threshold));
        taxInputs.OfType<FormulaVariable>()
            .Should()
            .Contain(v =>
                v.Name == nameof(LaborLawRuleKey.TaxBracket6Rate) &&
                Equals(v.Value, PayrollFormulaCatalog.TaxBracket6Rate));

        // The deprecated single-rate tax inputs are gone.
        taxInputs.OfType<FormulaVariable>().Should().NotContain(v => v.Name == "GrossAmount");
        taxInputs.OfType<FormulaVariable>().Should().NotContain(v => v.Name == nameof(LaborLawRuleKey.TaxExemptMonthlyAmount));
        taxInputs.OfType<FormulaVariable>().Should().NotContain(v => v.Name == nameof(LaborLawRuleKey.TaxRatePercentage));
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

        // 100 persisted + 24 current-period worked days = 124 annual total.
        var endOfServiceInputs = calls.Single(c => c.Expression == PayrollFormulaCatalog.Expression(FormulaKey.EndOfServicePay)).Inputs;
        endOfServiceInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "AnnualWorkedDaysCount" && Equals(v.Value, 124m));
        endOfServiceInputs.OfType<FormulaVariable>()
            .Should()
            .ContainSingle(v => v.Name == "DaysInYear" && Equals(v.Value, 365));

        // The annual variables reach only the year-proportional items.
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
    public async Task CalculateAsync_WhenTheDecreeAllowancesAreNull_ShouldEvaluateTheAllowanceFormulasToZero()
    {
        var calls = CaptureEvaluationCalls();

        await Calculate();

        // The fixture decree has no attraction/supervision allowances (null), and
        // the real evaluator normalizes null decimals to zero.
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
        // The taxable-base and tax formulas are evaluated with the real evaluator,
        // everything else keeps the fixed stub amount.
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
        // 13 taxable formula items (all but mission/end-of-service/annual bonus) at
        // 1,000,000 each + 500,000 + 200,000 optional = 13,700,000, which sits in
        // the first (exempt) tax bracket, so the progressive formula yields zero.
        response.Amounts.CalculatedTaxAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateAsync_WhenABracketRuleValueChanges_ShouldChangeTheTaxWithoutAnyCodeChange()
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

        // Taxable base is 13,700,000; a zero first-bracket threshold makes the
        // whole base taxable at the 10% second-bracket rate => 1,370,000.
        SetupRule(LaborLawRuleKey.TaxBracket1Threshold, 0m);

        var result = await Calculate();

        var response = result.ShouldBeSuccess();
        response.Amounts.CalculatedTaxAmount.Should().Be(1_370_000m);
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
        // The first Evaluate call is for a payroll item (e.g. BaseSalaryPay), which
        // receives the work input directly — unlike the insurance/tax calls that
        // only receive GrossAmount/TaxableAmount and labor law rule values.
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
        // The first Evaluate call is for a payroll item, which receives the
        // salary decree directly — insurance/tax calls do not.
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
