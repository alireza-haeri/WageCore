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

    public async Task<Result<PayrollCalculationResult>> CalculateAsync(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<SalaryDecree> salaryProfiles,
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

        if (salaryProfiles is null || salaryProfiles.Count == 0)
            return Result<PayrollCalculationResult>.NotfoundFailure("برای این بازه حکم حقوقی کارمند یافت نشد.");

        var salaryProfile = salaryProfiles
            .Where(profile => profile.EffectiveFrom <= periodStart)
            .OrderByDescending(profile => profile.EffectiveFrom)
            .FirstOrDefault() ?? salaryProfiles[^1];

        var period = new PayrollPeriod(
            periodStart,
            periodEnd,
            periodEnd.DayNumber - periodStart.DayNumber + 1,
            persianCalendarService.GetFridayCount(periodStart, periodEnd));

        logger.LogInformation(
            "Starting payroll calculation for employee {EmployeeId} ({EmployeeName}) in period {PeriodStart}..{PeriodEnd}",
            employee.Id,
            employee.FullName,
            periodStart,
            periodEnd);

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
                salaryProfile,
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

        var insuranceAmount = insuranceResult.Response;
        var calculatedTaxAmount = taxResult.Response;
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
            AnnualBonusAmount: GetAmount(amounts, FormulaKey.AnnualBonusPay),
            CommutingAllowanceAmount: GetAmount(amounts, FormulaKey.CommutingAllowancePay));

        var payrollAmounts = new PayrollRecordAmountsDto(
            CalculatedTaxAmount: calculatedTaxAmount ?? 0m,
            GrossAmount: grossAmount,
            InsuranceAmount: insuranceAmount ?? 0m,
            TotalDeductionsAmount: totalDeductionsAmount ?? 0m,
            NetPayableAmount: netPayableAmount ?? 0m);

        return Result<PayrollCalculationResult>.Success(
            new PayrollCalculationResult(calculatedAmounts, payrollAmounts));
    }

    private async Task<Result<decimal?>> CalculateItemAsync(
        CalculationItem item,
        Employee employee,
        Workshop workshop,
        SalaryDecree salaryProfile,
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
            ruleValue = await laborLawRuleQuery.GetActiveValueAsync(
                ruleKey.Value,
                period.PeriodStart,
                cancellationToken);
            if (ruleValue is null)
            {
                logger.LogWarning(
                    "Labor law rule {RuleKey} was not found for item {ItemName} of employee {EmployeeId} " +
                    "in period {PeriodStart}..{PeriodEnd}",
                    ruleKey.Value,
                    item.DisplayName,
                    employee.Id,
                    period.PeriodStart,
                    period.PeriodEnd);

                return Result<decimal?>.NotfoundFailure(
                    $"قانون {ruleKey.Value} برای محاسبه {item.DisplayName} یافت نشد.");
            }
        }

        var expression = await calculationFormulaQuery.GetActiveExpressionAsync(
            item.FormulaKey,
            period.PeriodStart,
            cancellationToken);
        if (expression is null)
        {
            logger.LogWarning(
                "Calculation formula {FormulaKey} was not found for item {ItemName} of employee {EmployeeId} " +
                "in period {PeriodStart}..{PeriodEnd}",
                item.FormulaKey,
                item.DisplayName,
                employee.Id,
                period.PeriodStart,
                period.PeriodEnd);

            return Result<decimal?>.NotfoundFailure(
                $"فرمول {item.FormulaKey} برای محاسبه {item.DisplayName} یافت نشد.");
        }

        var evaluationResult = formulaEvaluator.Evaluate(
            expression,
            BuildEvaluationInputs(item, employee, workshop, salaryProfile, workInput, period, ruleKey, ruleValue));
        if (!evaluationResult.IsSuccess)
        {
            logger.LogError(
                "Formula evaluation failed for item {ItemName} of employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}: {Error}",
                item.DisplayName,
                employee.Id,
                period.PeriodStart,
                period.PeriodEnd,
                evaluationResult.ErrorMessage);

            return Result<decimal?>.GeneralFailure(
                $"خطا در محاسبه {item.DisplayName}: {evaluationResult.ErrorMessage}");
        }

        return Result<decimal?>.Success(evaluationResult.Response);
    }

    private async Task<Result<decimal>> CalculateInsuranceAmountAsync(
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

        var insurancePercentage = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.InsurancePercentage,
            period.PeriodStart,
            cancellationToken);
        if (insurancePercentage is null)
        {
            logger.LogWarning(
                "Labor law rule {RuleKey} was not found for item {ItemName} of employee {EmployeeId} " +
                "in period {PeriodStart}..{PeriodEnd}",
                LaborLawRuleKey.InsurancePercentage,
                "بیمه ۷٪",
                employeeId,
                period.PeriodStart,
                period.PeriodEnd);

            return Result<decimal>.NotfoundFailure(
                $"قانون {LaborLawRuleKey.InsurancePercentage} برای محاسبه بیمه یافت نشد.");
        }

        var insuranceAmount = grossAmount * insurancePercentage.Value / 100m;
        logger.LogInformation(
            "Insurance amount calculated as {InsuranceAmount} for employee {EmployeeId} " +
            "(gross {GrossAmount} at {InsurancePercentage}%)",
            insuranceAmount,
            employeeId,
            grossAmount,
            insurancePercentage.Value);

        return Result<decimal>.Success(insuranceAmount);
    }

    private async Task<Result<decimal>> CalculateTaxAmountAsync(
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

        var taxExemptMonthlyAmount = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.TaxExemptMonthlyAmount,
            period.PeriodStart,
            cancellationToken);
        if (taxExemptMonthlyAmount is null)
        {
            logger.LogWarning(
                "Labor law rule {RuleKey} was not found for item {ItemName} of employee {EmployeeId} " +
                "in period {PeriodStart}..{PeriodEnd}",
                LaborLawRuleKey.TaxExemptMonthlyAmount,
                "مالیات",
                employeeId,
                period.PeriodStart,
                period.PeriodEnd);

            return Result<decimal>.NotfoundFailure(
                $"قانون {LaborLawRuleKey.TaxExemptMonthlyAmount} برای محاسبه مالیات یافت نشد.");
        }

        var taxRatePercentage = await laborLawRuleQuery.GetActiveValueAsync(
            LaborLawRuleKey.TaxRatePercentage,
            period.PeriodStart,
            cancellationToken);
        if (taxRatePercentage is null)
        {
            logger.LogWarning(
                "Labor law rule {RuleKey} was not found for item {ItemName} of employee {EmployeeId} " +
                "in period {PeriodStart}..{PeriodEnd}",
                LaborLawRuleKey.TaxRatePercentage,
                "مالیات",
                employeeId,
                period.PeriodStart,
                period.PeriodEnd);

            return Result<decimal>.NotfoundFailure(
                $"قانون {LaborLawRuleKey.TaxRatePercentage} برای محاسبه مالیات یافت نشد.");
        }

        var taxableAmount = grossAmount - taxExemptMonthlyAmount.Value;
        var calculatedTaxAmount = taxableAmount > 0
            ? taxableAmount * taxRatePercentage.Value / 100m
            : 0m;

        logger.LogInformation(
            "Tax amount calculated as {CalculatedTaxAmount} for employee {EmployeeId} " +
            "(gross {GrossAmount}, exempt {TaxExemptMonthlyAmount}, rate {TaxRatePercentage}%)",
            calculatedTaxAmount,
            employeeId,
            grossAmount,
            taxExemptMonthlyAmount.Value,
            taxRatePercentage.Value);

        return Result<decimal>.Success(calculatedTaxAmount);
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
        SalaryDecree salaryProfile,
        PayrollWorkInputDto workInput,
        PayrollPeriod period,
        LaborLawRuleKey? ruleKey,
        decimal? ruleValue)
    {
        var inputs = new List<object>
        {
            workInput,
            salaryProfile,
            employee,
            workshop,
            period
        };

        if (ruleKey is not null && ruleValue is not null)
            inputs.Add(new FormulaVariable(ruleKey.Value.ToString(), ruleValue.Value));

        if (item.FormulaKey == FormulaKey.ShiftWorkPay)
            inputs.Add(new FormulaVariable("ShiftType", (int)salaryProfile.ShiftType));

        if (item.FormulaKey == FormulaKey.MarriageAllowancePay)
            inputs.Add(new FormulaVariable("MaritalStatus", (int)salaryProfile.MaritalStatus));

        return inputs.ToArray();
    }

    private static decimal GetAmount(IReadOnlyDictionary<FormulaKey, decimal> amounts, FormulaKey key) =>
        amounts.TryGetValue(key, out var amount) ? amount : 0m;

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
