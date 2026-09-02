using Core.Contracts.CalculationFormulas;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PayrollCalculationService(
    ILaborLawRuleQuery laborLawRuleQuery,
    ICalculationFormulaQuery calculationFormulaQuery,
    IFormulaEvaluator formulaEvaluator,
    IPersianCalendarService persianCalendarService,
    IPayrollRecordQuery payrollRecordQuery,
    ILogger<PayrollCalculationService> logger)
    : IPayrollCalculationService
{
    // Every monetary amount of a payroll period is derived from formulas: the item
    // rows below declare, per component, which labor law rule values the formula
    // receives (the formula itself is stored per FormulaKey and resolved at run
    // time, so no business constant lives in this code).
    private static readonly CalculationItem[] Items =
    [
        new("پایه حقوق ماهانه", FormulaKey.BaseSalaryPay, []),
        new("حق جذب", FormulaKey.AttractionAllowancePay, []),
        new("حق سرپرستی", FormulaKey.SupervisionAllowancePay, []),
        new("فوق‌العاده شب‌کاری", FormulaKey.NightShiftExtraPay, [LaborLawRuleKey.NightShiftPercentage, LaborLawRuleKey.StandardDailyWorkHours]),
        new("مبلغ تعطیل‌کاری", FormulaKey.HolidayWorkPay, [LaborLawRuleKey.HolidayWorkPercentage, LaborLawRuleKey.StandardDailyWorkHours]),
        new("حق اولاد", FormulaKey.ChildAllowancePay, [LaborLawRuleKey.MinimumDailySalary, LaborLawRuleKey.ChildAllowanceMultiplier]),
        new("هزینه مسکن", FormulaKey.HousingAllowancePay, [LaborLawRuleKey.HousingAllowanceAmount]),
        new("حق بن و خوار و بار", FormulaKey.FoodAllowancePay, [LaborLawRuleKey.FoodAllowanceAmount]),
        new("حق تأهل", FormulaKey.MarriageAllowancePay, [LaborLawRuleKey.MarriageAllowanceAmount]),
        new("مبلغ اضافه‌کاری", FormulaKey.OvertimePay, [LaborLawRuleKey.OvertimePercentage, LaborLawRuleKey.StandardDailyWorkHours]),
        new("مبلغ نوبت‌کاری", FormulaKey.ShiftWorkPay, []),
        new("مبلغ مأموریت روزانه", FormulaKey.DailyMissionPay, []),
        new("حق کار جمعه", FormulaKey.FridayWorkPay, [LaborLawRuleKey.FridayWorkPercentage, LaborLawRuleKey.StandardDailyWorkHours]),
        new("مبلغ سنوات پایان سال", FormulaKey.EndOfServicePay, [LaborLawRuleKey.EndOfServiceDaysPerYear]),
        new("مبلغ عیدی سالانه", FormulaKey.AnnualBonusPay, []),
        new("مبلغ ایاب و ذهاب", FormulaKey.CommutingAllowancePay, [])
    ];

    private static readonly LaborLawRuleKey[] TaxBracketRuleKeys =
    [
        LaborLawRuleKey.TaxBracket1Threshold,
        LaborLawRuleKey.TaxBracket2Threshold,
        LaborLawRuleKey.TaxBracket2Rate,
        LaborLawRuleKey.TaxBracket3Threshold,
        LaborLawRuleKey.TaxBracket3Rate,
        LaborLawRuleKey.TaxBracket4Threshold,
        LaborLawRuleKey.TaxBracket4Rate,
        LaborLawRuleKey.TaxBracket5Threshold,
        LaborLawRuleKey.TaxBracket5Rate,
        LaborLawRuleKey.TaxBracket6Rate
    ];

    // Calculates every monetary amount of a payroll period: the salary decree
    // effective for the period is selected first, then each payroll item is
    // calculated from its formula, optional entered amounts are added, and
    // finally insurance, tax and the totals are derived via formulas as well.
    public async Task<Result<PayrollCalculationResult>> CalculateAsync(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<SalaryDecree> salaryDecrees,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInputDto workInput,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (workInput is null)
            return Result<PayrollCalculationResult>.GeneralFailure("اطلاعات کارکرد کارمند نمیتواند خالی باشد.");

        if (employee is null)
            return Result<PayrollCalculationResult>.GeneralFailure("کارمند نمیتواند خالی باشد.");

        if (workshop is null)
            return Result<PayrollCalculationResult>.GeneralFailure("کارگاه نمیتواند خالی باشد.");

        if (salaryDecrees is null || salaryDecrees.Count == 0)
            return Result<PayrollCalculationResult>.NotfoundFailure("برای این بازه حکم حقوقی کارمند یافت نشد.");

        // A decree is in force for the whole period once its effective date has
        // passed, so the latest decree effective by the period end is selected.
        var salaryDecree = salaryDecrees
            .Where(decree => decree.EffectiveFrom <= periodEnd)
            .OrderByDescending(decree => decree.EffectiveFrom)
            .FirstOrDefault();

        if (salaryDecree is null)
        {
            logger.LogWarning(
                "No active salary decree found for employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}",
                employee.Id,
                periodStart,
                periodEnd);

            return Result<PayrollCalculationResult>.NotfoundFailure(
                "حکم حقوقی فعال برای این کارمند در این بازه یافت نشد.");
        }

        var period = new PayrollPeriod(
            periodStart,
            periodEnd,
            periodEnd.DayNumber - periodStart.DayNumber + 1,
            persianCalendarService.GetFridayCount(periodStart, periodEnd));

        var isEsfandPeriod = persianCalendarService.GetPersianMonth(periodStart) == 12;

        logger.LogInformation(
            "Starting payroll calculation for employee {EmployeeId} ({EmployeeName}) in period {PeriodStart}..{PeriodEnd}",
            employee.Id,
            employee.FullName,
            periodStart,
            periodEnd);

        // Annual context shared by the year-proportional items (end-of-service and
        // annual bonus): the length of the Persian year containing the period and
        // the days worked so far in that year. The query only aggregates closed
        // (already-persisted) periods, so the current period's worked days are
        // added in code to obtain the true annual total.
        var daysInYear = persianCalendarService.GetDaysInPersianYear(periodStart);

        var previousWorkedDaysCount = await payrollRecordQuery.GetAnnualWorkedDaysCountAsync(
            workshop.UserId,
            employee.Id,
            periodStart,
            cancellationToken);

        var annualWorkedDaysCount = previousWorkedDaysCount + (workInput.WorkedDaysCount ?? 0m);

        logger.LogInformation(
            "Annual context for employee {EmployeeId}: year has {DaysInYear} days and " +
            "{AnnualWorkedDaysCount} worked days in total ({PreviousWorkedDaysCount} persisted + {CurrentWorkedDaysCount} current)",
            employee.Id,
            daysInYear,
            annualWorkedDaysCount,
            previousWorkedDaysCount,
            workInput.WorkedDaysCount ?? 0m);

        // Each CalculationItem maps one payroll component to its formula; when
        // the item is not applicable for this period its result is null and it
        // is skipped (a skipped item is not the same as a zero amount).
        var amounts = new Dictionary<FormulaKey, decimal>();
        foreach (var item in Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation(
                "Starting calculation of item {ItemName} for employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}",
                item.DisplayName,
                employee.Id,
                periodStart,
                periodEnd);

            var itemResult = await CalculateItemAsync(
                item,
                employee,
                workshop,
                salaryDecree,
                workInput,
                period,
                daysInYear,
                annualWorkedDaysCount,
                cancellationToken);
            if (!itemResult.IsSuccess)
                return ConvertFailure(itemResult);

            var amount = itemResult.Response;
            if (amount is null)
                continue;

            amounts[item.FormulaKey] = amount.Value;
            logger.LogInformation(
                "Item {ItemName} calculated as {Amount} for employee {EmployeeId}",
                item.DisplayName,
                amount.Value,
                employee.Id);
        }

        var optionalAmounts = new List<decimal>();
        var performanceBonusResult = AddOptionalAmount(
            workInput.PerformanceBonusAmount,
            "کارانه",
            employee.Id,
            cancellationToken);
        if (!performanceBonusResult.IsSuccess)
            return ConvertFailure(performanceBonusResult);
        if (performanceBonusResult.Response is { } performanceBonusAmount)
            optionalAmounts.Add(performanceBonusAmount);

        var cashBenefitsResult = AddOptionalAmount(
            workInput.CashBenefitsAmount,
            "مزایای نقدی",
            employee.Id,
            cancellationToken);
        if (!cashBenefitsResult.IsSuccess)
            return ConvertFailure(cashBenefitsResult);
        if (cashBenefitsResult.Response is { } cashBenefitsAmount)
            optionalAmounts.Add(cashBenefitsAmount);

        var grossAmount = amounts.Values.Sum() + optionalAmounts.Sum();
        logger.LogInformation(
            "Gross amount calculated as {GrossAmount} for employee {EmployeeId}",
            grossAmount,
            employee.Id);

        var insuranceResult = await CalculateInsuranceAmountAsync(
            grossAmount,
            employee.Id,
            period,
            cancellationToken);
        if (!insuranceResult.IsSuccess)
            return ConvertFailure(insuranceResult);

        var taxResult = await CalculateTaxAmountAsync(
            amounts,
            performanceBonusResult.Response,
            cashBenefitsResult.Response,
            employee.Id,
            period,
            cancellationToken);
        if (!taxResult.IsSuccess)
            return ConvertFailure(taxResult);

        // The IsSuccess guards above guarantee these values are non-null; .Value is
        // used instead of ?? 0m so a missing amount can never be silently masked as zero.
        var insuranceAmount = insuranceResult.Response!.Value;
        var calculatedTaxAmount = taxResult.Response!.Value;
        var totalDeductionsAmount = insuranceAmount + calculatedTaxAmount;
        var netPayableAmount = grossAmount - totalDeductionsAmount;

        logger.LogInformation(
            "Totals calculated for employee {EmployeeId}: GrossAmount {GrossAmount}, InsuranceAmount {InsuranceAmount}, " +
            "CalculatedTaxAmount {CalculatedTaxAmount}, TotalDeductionsAmount {TotalDeductionsAmount}, " +
            "NetPayableAmount {NetPayableAmount}",
            employee.Id,
            grossAmount,
            insuranceAmount,
            calculatedTaxAmount,
            totalDeductionsAmount,
            netPayableAmount);

        var calculatedAmounts = new PayrollCalculatedAmountsDto(
            BaseSalaryAmount: GetAmount(amounts, FormulaKey.BaseSalaryPay),
            AttractionAllowanceAmount: GetAmount(amounts, FormulaKey.AttractionAllowancePay),
            SupervisionAllowanceAmount: GetAmount(amounts, FormulaKey.SupervisionAllowancePay),
            NightShiftExtraAmount: GetAmount(amounts, FormulaKey.NightShiftExtraPay),
            HolidayWorkAmount: GetAmount(amounts, FormulaKey.HolidayWorkPay),
            ChildAllowanceAmount: GetAmount(amounts, FormulaKey.ChildAllowancePay),
            HousingAllowanceAmount: GetAmount(amounts, FormulaKey.HousingAllowancePay),
            FoodAllowanceAmount: GetAmount(amounts, FormulaKey.FoodAllowancePay),
            MarriageAllowanceAmount: GetAmount(amounts, FormulaKey.MarriageAllowancePay),
            OvertimeAmount: GetAmount(amounts, FormulaKey.OvertimePay),
            ShiftWorkAmount: GetAmount(amounts, FormulaKey.ShiftWorkPay),
            DailyMissionAmount: GetAmount(amounts, FormulaKey.DailyMissionPay),
            FridayWorkAllowance: GetAmount(amounts, FormulaKey.FridayWorkPay),
            EndOfServiceAmount: GetAmount(amounts, FormulaKey.EndOfServicePay),
            AnnualBonusAmount: GetOptionalAmount(amounts, FormulaKey.AnnualBonusPay),
            CommutingAllowanceAmount: GetAmount(amounts, FormulaKey.CommutingAllowancePay),
            PerformanceBonusAmount: performanceBonusResult.Response,
            CashBenefitsAmount: cashBenefitsResult.Response);

        var payrollAmounts = new PayrollRecordAmountsDto(
            CalculatedTaxAmount: calculatedTaxAmount,
            GrossAmount: grossAmount,
            InsuranceAmount: insuranceAmount,
            TotalDeductionsAmount: totalDeductionsAmount,
            NetPayableAmount: netPayableAmount);

        return Result<PayrollCalculationResult>.Success(
            new PayrollCalculationResult(calculatedAmounts, payrollAmounts, isEsfandPeriod));
    }

    private async Task<Result<decimal?>> CalculateItemAsync(
        CalculationItem item,
        Employee employee,
        Workshop workshop,
        SalaryDecree salaryDecree,
        PayrollWorkInputDto workInput,
        PayrollPeriod period,
        int daysInYear,
        decimal annualWorkedDaysCount,
        CancellationToken cancellationToken)
    {
        if (item.FormulaKey == FormulaKey.DailyMissionPay && workInput.MissionAmountOverride is not null)
        {
            logger.LogInformation(
                "Mission amount override {MissionAmountOverride} is provided for employee {EmployeeId}; " +
                "using it directly for {ItemName} instead of evaluating the formula",
                workInput.MissionAmountOverride.Value,
                employee.Id,
                item.DisplayName);

            return Result<decimal?>.Success(workInput.MissionAmountOverride.Value);
        }

        if (item.FormulaKey == FormulaKey.AnnualBonusPay && workInput.AnnualBonusType is null)
        {
            logger.LogInformation(
                "Annual bonus type is not set for employee {EmployeeId}; skipping {ItemName}",
                employee.Id,
                item.DisplayName);

            return Result<decimal?>.Success(null);
        }

        if (item.FormulaKey == FormulaKey.ShiftWorkPay && salaryDecree.ShiftType == ShiftType.None)
        {
            logger.LogInformation(
                "Shift type is None for employee {EmployeeId}; skipping {ItemName}",
                employee.Id,
                item.DisplayName);

            return Result<decimal?>.Success(null);
        }

        // Some components resolve their rule key from the payroll context instead
        // of a fixed key: the annual bonus reads the minimum or maximum rule and
        // shift work reads the rule of the decree's shift type.
        var ruleKeys = new List<LaborLawRuleKey>(item.RuleKeys);
        if (item.FormulaKey == FormulaKey.AnnualBonusPay)
        {
            ruleKeys.Add(workInput.AnnualBonusType == AnnualBonusType.Minimum
                ? LaborLawRuleKey.AnnualBonusMinimumAmount
                : LaborLawRuleKey.AnnualBonusMaximumAmount);
        }
        else if (item.FormulaKey == FormulaKey.ShiftWorkPay)
        {
            ruleKeys.Add(GetShiftWorkRuleKey(salaryDecree.ShiftType));
        }

        var ruleValues = new List<(LaborLawRuleKey Key, decimal Value)>();
        foreach (var ruleKey in ruleKeys)
        {
            var ruleResult = await GetRuleValueAsync(
                ruleKey,
                item.DisplayName,
                employee.Id,
                period,
                cancellationToken);
            if (!ruleResult.IsSuccess)
                return ruleResult.Map<decimal?>(value => value);

            ruleValues.Add((ruleKey, ruleResult.Response));
        }

        return await EvaluateFormulaAsync(
            item.FormulaKey,
            item.DisplayName,
            employee.Id,
            period,
            BuildEvaluationInputs(item, employee, workshop, salaryDecree, workInput, period, ruleValues, daysInYear, annualWorkedDaysCount),
            cancellationToken);
    }

    // Maps a non-None shift type to its shift-work percentage rule key.
    private static LaborLawRuleKey GetShiftWorkRuleKey(ShiftType shiftType) =>
        shiftType switch
        {
            ShiftType.MorningEvening => LaborLawRuleKey.ShiftWorkPercentageMorningEvening,
            ShiftType.MorningNight => LaborLawRuleKey.ShiftWorkPercentageMorningNight,
            ShiftType.EveningNight => LaborLawRuleKey.ShiftWorkPercentageEveningNight,
            ShiftType.MorningEveningNight => LaborLawRuleKey.ShiftWorkPercentageMorningEveningNight,
            _ => throw new ArgumentOutOfRangeException(nameof(shiftType), shiftType, null)
        };

    // Rule values reach the formula under a stable variable name the expression can
    // reference regardless of which specific rule row was resolved for the item
    // (annual bonus and shift work each have several candidate rule keys).
    private static string GetRuleVariableName(FormulaKey formulaKey, LaborLawRuleKey ruleKey) =>
        formulaKey switch
        {
            FormulaKey.AnnualBonusPay => "AnnualBonusRuleAmount",
            FormulaKey.ShiftWorkPay => "ShiftWorkPercentage",
            _ => ruleKey.ToString()
        };

    // Fetches the active value of a labor law rule; a missing rule is reported
    // as a not-found failure instead of silently calculating with zero.
    private async Task<Result<decimal>> GetRuleValueAsync(
        LaborLawRuleKey ruleKey,
        string displayName,
        Guid employeeId,
        PayrollPeriod period,
        CancellationToken cancellationToken)
    {
        var ruleValue = await laborLawRuleQuery.GetActiveValueAsync(
            ruleKey,
            period.PeriodStart,
            cancellationToken);
        if (ruleValue is null)
        {
            logger.LogWarning(
                "Labor law rule {RuleKey} was not found for {ItemName} of employee {EmployeeId} " +
                "in period {PeriodStart}..{PeriodEnd}",
                ruleKey,
                displayName,
                employeeId,
                period.PeriodStart,
                period.PeriodEnd);

            return Result<decimal>.NotfoundFailure(
                $"قانون {ruleKey} برای محاسبه {displayName} یافت نشد.");
        }

        return Result<decimal>.Success(ruleValue.Value);
    }

    // Shared by payroll items, insurance and tax: fetches the formula expression
    // for a key and evaluates it with the given inputs.
    private async Task<Result<decimal?>> EvaluateFormulaAsync(
        FormulaKey formulaKey,
        string displayName,
        Guid employeeId,
        PayrollPeriod period,
        object[] inputs,
        CancellationToken cancellationToken)
    {
        var expression = await calculationFormulaQuery.GetActiveExpressionAsync(
            formulaKey,
            period.PeriodStart,
            cancellationToken);
        if (expression is null)
        {
            logger.LogWarning(
                "Calculation formula {FormulaKey} was not found for {ItemName} of employee {EmployeeId} " +
                "in period {PeriodStart}..{PeriodEnd}",
                formulaKey,
                displayName,
                employeeId,
                period.PeriodStart,
                period.PeriodEnd);

            return Result<decimal?>.NotfoundFailure(
                $"فرمول {formulaKey} برای محاسبه {displayName} یافت نشد.");
        }

        var evaluationResult = formulaEvaluator.Evaluate(expression, inputs);
        if (!evaluationResult.IsSuccess)
        {
            logger.LogError(
                "Formula evaluation failed for {ItemName} of employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}: {Error}",
                displayName,
                employeeId,
                period.PeriodStart,
                period.PeriodEnd,
                evaluationResult.ErrorMessage);

            return Result<decimal?>.GeneralFailure(
                $"خطا در محاسبه {displayName}: {evaluationResult.ErrorMessage}");
        }

        return Result<decimal?>.Success(evaluationResult.Response);
    }

    private async Task<Result<decimal?>> CalculateInsuranceAmountAsync(
        decimal grossAmount,
        Guid employeeId,
        PayrollPeriod period,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting calculation of insurance amount for employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}",
            employeeId,
            period.PeriodStart,
            period.PeriodEnd);

        var insurancePercentageResult = await GetRuleValueAsync(
            LaborLawRuleKey.InsurancePercentage,
            "بیمه",
            employeeId,
            period,
            cancellationToken);
        if (!insurancePercentageResult.IsSuccess)
            return insurancePercentageResult.Map<decimal?>(value => value);

        var insurancePercentage = insurancePercentageResult.Response;

        // The insurance formula receives the gross amount and the insurance
        // percentage rule value, e.g. "GrossAmount * InsurancePercentage / 100".
        var insuranceResult = await EvaluateFormulaAsync(
            FormulaKey.InsurancePay,
            "بیمه",
            employeeId,
            period,
            [
                new FormulaVariable("GrossAmount", grossAmount),
                new FormulaVariable(nameof(LaborLawRuleKey.InsurancePercentage), insurancePercentage)
            ],
            cancellationToken);
        if (!insuranceResult.IsSuccess)
            return insuranceResult;

        logger.LogInformation(
            "Insurance amount calculated as {InsuranceAmount} for employee {EmployeeId} " +
            "(gross {GrossAmount} at {InsurancePercentage}%)",
            insuranceResult.Response,
            employeeId,
            grossAmount,
            insurancePercentage);

        return insuranceResult;
    }

    // Tax is fully formula-driven in two steps: the taxable base formula first
    // sums the taxable item amounts, then the tax formula applies the progressive
    // brackets whose thresholds/rates all come from labor law rules.
    private async Task<Result<decimal?>> CalculateTaxAmountAsync(
        IReadOnlyDictionary<FormulaKey, decimal> amounts,
        decimal? performanceBonusAmount,
        decimal? cashBenefitsAmount,
        Guid employeeId,
        PayrollPeriod period,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting calculation of tax amount for employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}",
            employeeId,
            period.PeriodStart,
            period.PeriodEnd);

        var taxableAmountResult = await EvaluateFormulaAsync(
            FormulaKey.TaxableAmountPay,
            "مالیات",
            employeeId,
            period,
            BuildTaxableAmountInputs(amounts, performanceBonusAmount, cashBenefitsAmount),
            cancellationToken);
        if (!taxableAmountResult.IsSuccess)
            return taxableAmountResult;

        var taxableAmount = taxableAmountResult.Response!.Value;

        var ruleValues = new List<(LaborLawRuleKey Key, decimal Value)>();
        foreach (var ruleKey in TaxBracketRuleKeys)
        {
            var ruleResult = await GetRuleValueAsync(
                ruleKey,
                "مالیات",
                employeeId,
                period,
                cancellationToken);
            if (!ruleResult.IsSuccess)
                return ruleResult.Map<decimal?>(value => value);

            ruleValues.Add((ruleKey, ruleResult.Response));
        }

        // The tax formula receives the taxable base and every bracket threshold/
        // rate rule, e.g. the progressive nested-ternary expression over
        // "[TaxableAmount] <= [TaxBracket1Threshold] ? 0 : ...".
        var taxInputs = new List<FormulaVariable> { new("TaxableAmount", taxableAmount) };
        taxInputs.AddRange(ruleValues.Select(rule => new FormulaVariable(rule.Key.ToString(), rule.Value)));

        var taxResult = await EvaluateFormulaAsync(
            FormulaKey.TaxPay,
            "مالیات",
            employeeId,
            period,
            taxInputs.ToArray(),
            cancellationToken);
        if (!taxResult.IsSuccess)
            return taxResult;

        logger.LogInformation(
            "Tax amount calculated as {CalculatedTaxAmount} for employee {EmployeeId} " +
            "(taxable amount {TaxableAmount})",
            taxResult.Response,
            employeeId,
            taxableAmount);

        return taxResult;
    }

    // Builds the inputs of the taxable-base formula: one variable per payroll
    // item amount already computed (mission and end-of-service are excluded by
    // the formula expression) plus the optional performance/cash amounts.
    private static object[] BuildTaxableAmountInputs(
        IReadOnlyDictionary<FormulaKey, decimal> amounts,
        decimal? performanceBonusAmount,
        decimal? cashBenefitsAmount)
    {
        var taxableItemKeys = Items
            .Select(item => item.FormulaKey)
            .Where(key => key != FormulaKey.DailyMissionPay && key != FormulaKey.EndOfServicePay);

        var inputs = taxableItemKeys
            .Select(key => new FormulaVariable(key.ToString(), GetAmount(amounts, key)))
            .Cast<object>()
            .ToList();

        inputs.Add(new FormulaVariable("PerformanceBonusAmount", performanceBonusAmount ?? 0m));
        inputs.Add(new FormulaVariable("CashBenefitsAmount", cashBenefitsAmount ?? 0m));

        return inputs.ToArray();
    }

    private Result<decimal?> AddOptionalAmount(
        decimal? value,
        string displayName,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (value is null)
        {
            logger.LogInformation(
                "{ItemName} is not applicable for employee {EmployeeId}; skipped",
                displayName,
                employeeId);

            return Result<decimal?>.Success(null);
        }

        logger.LogInformation(
            "Using entered {ItemName} amount {Amount} for employee {EmployeeId}",
            displayName,
            value.Value,
            employeeId);

        return Result<decimal?>.Success(value.Value);
    }

    private static object[] BuildEvaluationInputs(
        CalculationItem item,
        Employee employee,
        Workshop workshop,
        SalaryDecree salaryDecree,
        PayrollWorkInputDto workInput,
        PayrollPeriod period,
        IReadOnlyList<(LaborLawRuleKey Key, decimal Value)> ruleValues,
        int daysInYear,
        decimal annualWorkedDaysCount)
    {
        var inputs = new List<object>
        {
            workInput,
            salaryDecree,
            employee,
            workshop,
            period
        };

        foreach (var ruleValue in ruleValues)
            inputs.Add(new FormulaVariable(GetRuleVariableName(item.FormulaKey, ruleValue.Key), ruleValue.Value));

        if (item.FormulaKey == FormulaKey.MarriageAllowancePay)
            inputs.Add(new FormulaVariable("MaritalStatus", (int)salaryDecree.MaritalStatus));

        // The year-proportional items receive the annual context as variables.
        if (item.FormulaKey is FormulaKey.EndOfServicePay or FormulaKey.AnnualBonusPay)
        {
            inputs.Add(new FormulaVariable("DaysInYear", daysInYear));
            inputs.Add(new FormulaVariable("AnnualWorkedDaysCount", annualWorkedDaysCount));
        }

        return inputs.ToArray();
    }

    private static decimal GetAmount(IReadOnlyDictionary<FormulaKey, decimal> amounts, FormulaKey key) =>
        amounts.TryGetValue(key, out var amount) ? amount : 0m;

    private static decimal? GetOptionalAmount(IReadOnlyDictionary<FormulaKey, decimal> amounts, FormulaKey key) =>
        amounts.TryGetValue(key, out var amount) ? amount : (decimal?)null;

    private static Result<PayrollCalculationResult> ConvertFailure<T>(Result<T> result)
    {
        var message = result.Errors is not null &&
                      result.Errors.TryGetValue("General", out var messages) &&
                      messages.Length > 0
            ? messages[0]
            : "خطایی رخ داده است!";

        return result.BadResultType == BadResultType.NotFound
            ? Result<PayrollCalculationResult>.NotfoundFailure(message)
            : Result<PayrollCalculationResult>.GeneralFailure(message);
    }

    private sealed record CalculationItem(
        string DisplayName,
        FormulaKey FormulaKey,
        IReadOnlyList<LaborLawRuleKey> RuleKeys);

    private sealed record PayrollPeriod(
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        int PeriodDaysCount,
        int FridayCount);
}
