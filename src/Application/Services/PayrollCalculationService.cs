using Core.Contracts.CalculationFormulas;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PayrollCalculationService(
    ILaborLawRuleQuery laborLawRuleQuery,
    ICalculationFormulaQuery calculationFormulaQuery,
    IFormulaEvaluator formulaEvaluator,
    IPersianCalendarService persianCalendarService,
    ILogger<PayrollCalculationService> logger)
    : IPayrollCalculationService
{
    private static readonly CalculationItem[] Items =
    [
        new("پایه حقوق ماهانه", FormulaKey.BaseSalaryPay, null),
        new("حق جذب", FormulaKey.AttractionAllowancePay, null),
        new("حق سرپرستی", FormulaKey.SupervisionAllowancePay, null),
        new("فوق‌العاده شب‌کاری", FormulaKey.NightShiftExtraPay, null),
        new("مبلغ تعطیل‌کاری", FormulaKey.HolidayWorkPay, null),
        new("حق اولاد", FormulaKey.ChildAllowancePay, null),
        new("هزینه مسکن", FormulaKey.HousingAllowancePay, null),
        new("حق بن و خوار و بار", FormulaKey.FoodAllowancePay, null),
        new("حق تأهل", FormulaKey.MarriageAllowancePay, null),
        new("مبلغ اضافه‌کاری", FormulaKey.OvertimePay, LaborLawRuleKey.MaximumOvertimeHoursPerMonth),
        new("مبلغ نوبت‌کاری", FormulaKey.ShiftWorkPay, null),
        new("مبلغ مأموریت روزانه", FormulaKey.DailyMissionPay, null),
        new("حق کار جمعه", FormulaKey.FridayWorkPay, LaborLawRuleKey.MaximumFridayWorkHoursPerMonth),
        new("مبلغ سنوات پایان سال", FormulaKey.EndOfServicePay, null),
        new("مبلغ عیدی سالانه", FormulaKey.AnnualBonusPay, null),
        new("مبلغ ایاب و ذهاب", FormulaKey.CommutingAllowancePay, null)
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
            grossAmount,
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

        LaborLawRuleKey? ruleKey = item.RuleKey;
        if (item.FormulaKey == FormulaKey.AnnualBonusPay)
        {
            ruleKey = workInput.AnnualBonusType == AnnualBonusType.Minimum
                ? LaborLawRuleKey.AnnualBonusMinimumAmount
                : LaborLawRuleKey.AnnualBonusMaximumAmount;
        }

        decimal? ruleValue = null;
        if (ruleKey is not null)
        {
            var ruleResult = await GetRuleValueAsync(
                ruleKey.Value,
                item.DisplayName,
                employee.Id,
                period,
                cancellationToken);
            if (!ruleResult.IsSuccess)
                return ruleResult.Map<decimal?>(value => value);

            ruleValue = ruleResult.Response;
        }

        return await EvaluateFormulaAsync(
            item.FormulaKey,
            item.DisplayName,
            employee.Id,
            period,
            BuildEvaluationInputs(item, employee, workshop, salaryDecree, workInput, period, ruleKey, ruleValue),
            cancellationToken);
    }

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

    private async Task<Result<decimal?>> CalculateTaxAmountAsync(
        decimal grossAmount,
        Guid employeeId,
        PayrollPeriod period,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting calculation of tax amount for employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}",
            employeeId,
            period.PeriodStart,
            period.PeriodEnd);

        var taxExemptAmountResult = await GetRuleValueAsync(
            LaborLawRuleKey.TaxExemptMonthlyAmount,
            "مالیات",
            employeeId,
            period,
            cancellationToken);
        if (!taxExemptAmountResult.IsSuccess)
            return taxExemptAmountResult.Map<decimal?>(value => value);

        var taxRateResult = await GetRuleValueAsync(
            LaborLawRuleKey.TaxRatePercentage,
            "مالیات",
            employeeId,
            period,
            cancellationToken);
        if (!taxRateResult.IsSuccess)
            return taxRateResult.Map<decimal?>(value => value);

        // The tax formula receives the gross amount, the tax-exempt monthly
        // amount and the tax rate rule values, e.g. a tiered multi-bracket
        // formula or "Max(GrossAmount - TaxExemptMonthlyAmount, 0) * TaxRatePercentage / 100".
        var taxResult = await EvaluateFormulaAsync(
            FormulaKey.TaxPay,
            "مالیات",
            employeeId,
            period,
            [
                new FormulaVariable("GrossAmount", grossAmount),
                new FormulaVariable(nameof(LaborLawRuleKey.TaxExemptMonthlyAmount), taxExemptAmountResult.Response),
                new FormulaVariable(nameof(LaborLawRuleKey.TaxRatePercentage), taxRateResult.Response)
            ],
            cancellationToken);
        if (!taxResult.IsSuccess)
            return taxResult;

        logger.LogInformation(
            "Tax amount calculated as {CalculatedTaxAmount} for employee {EmployeeId}",
            taxResult.Response,
            employeeId);

        return taxResult;
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
        LaborLawRuleKey? ruleKey,
        decimal? ruleValue)
    {
        var inputs = new List<object>
        {
            workInput,
            salaryDecree,
            employee,
            workshop,
            period
        };

        if (ruleKey is not null && ruleValue is not null)
            inputs.Add(new FormulaVariable(ruleKey.Value.ToString(), ruleValue.Value));

        if (item.FormulaKey == FormulaKey.ShiftWorkPay)
            inputs.Add(new FormulaVariable("ShiftType", (int)salaryDecree.ShiftType));

        if (item.FormulaKey == FormulaKey.MarriageAllowancePay)
            inputs.Add(new FormulaVariable("MaritalStatus", (int)salaryDecree.MaritalStatus));

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
        LaborLawRuleKey? RuleKey);

    private sealed record PayrollPeriod(
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        int PeriodDaysCount,
        int FridayCount);
}
