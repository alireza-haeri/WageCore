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
    private readonly ILogger<PayrollCalculationService> _logger;
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
        _logger = Substitute.For<ILogger<PayrollCalculationService>>();

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
        overtimeFormulaInputs!.Should().Contain(v => v is FormulaVariable variable &&
            variable.Name == nameof(LaborLawRuleKey.MaximumOvertimeHoursPerMonth) &&
            Equals(variable.Value, 80m));
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
            response.Amounts.GrossAmount.Should().Be(14 * DefaultItemAmount + 500_000m);
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

        result.ShouldBeFailure("قانون MaximumOvertimeHoursPerMonth", BadResultType.NotFound);
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<It.IsAnyType>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<It.IsAnyType, Exception?, string>>());
        await _calculationFormulaQuery.DidNotReceive()
            .GetActiveExpressionAsync(FormulaKey.OvertimePay, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculateAsync_WhenAFormulaIsMissing_ShouldReturnNotfoundFailureAndLog()
    {
        SetupFormula(FormulaKey.BaseSalaryPay, null);

        var result = await Calculate();

        result.ShouldBeFailure("فرمول BaseSalaryPay", BadResultType.NotFound);
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<It.IsAnyType>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<It.IsAnyType, Exception?, string>>());
    }

    [Fact]
    public async Task CalculateAsync_WhenTheFormulaEvaluationFails_ShouldReturnGeneralFailureAndLog()
    {
        SetupEvaluationFailure("خطای آزمون");

        var result = await Calculate();

        result.ShouldBeFailure("خطا در محاسبه", BadResultType.General);
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<It.IsAnyType>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<It.IsAnyType, Exception?, string>>());
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
        var grossAmount = 15 * DefaultItemAmount + 500_000m + 200_000m;
        var insuranceAmount = grossAmount * 7m / 100m;
        var taxAmount = grossAmount * 10m / 100m;
        var totalDeductionsAmount = insuranceAmount + taxAmount;

        using (new AssertionScope())
        {
            response.Amounts.GrossAmount.Should().Be(grossAmount);
            response.Amounts.InsuranceAmount.Should().Be(insuranceAmount);
            response.Amounts.CalculatedTaxAmount.Should().Be(taxAmount);
            response.Amounts.TotalDeductionsAmount.Should().Be(totalDeductionsAmount);
            response.Amounts.NetPayableAmount.Should().Be(grossAmount - totalDeductionsAmount);
        }
    }

    [Fact]
    public async Task CalculateAsync_WithoutOptionalAmounts_ShouldSkipThemAndExcludeThemFromGross()
    {
        var result = await Calculate();

        var response = result.ShouldBeSuccess();
        response.Amounts.GrossAmount.Should().Be(15 * DefaultItemAmount);
    }

    [Fact]
    public async Task CalculateAsync_ShouldUseTheFetchedInsurancePercentageForTheInsuranceAmount()
    {
        SetupRule(LaborLawRuleKey.InsurancePercentage, 8m);

        var result = await Calculate();

        result.ShouldBeSuccess().Amounts.InsuranceAmount.Should().Be(15 * DefaultItemAmount * 8m / 100m);
    }

    [Fact]
    public async Task CalculateAsync_ShouldUseTheFetchedTaxRulesForTheTaxAmount()
    {
        SetupRule(LaborLawRuleKey.TaxExemptMonthlyAmount, 2_000_000m);
        SetupRule(LaborLawRuleKey.TaxRatePercentage, 20m);

        var result = await Calculate();

        var expectedTax = (15 * DefaultItemAmount - 2_000_000m) * 20m / 100m;
        result.ShouldBeSuccess().Amounts.CalculatedTaxAmount.Should().Be(expectedTax);
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

        result.ShouldBeFailure("حکم حقوقی کارمند یافت نشد.", BadResultType.NotFound);
    }
}
