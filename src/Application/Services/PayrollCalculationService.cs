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

    public async Task<Result<PayrollCalculationResult>> CalculateAsync(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<SalaryDecree> salaryDecrees,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInput workInput,
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

        var daysInYear = persianCalendarService.GetDaysInPersianYear(periodStart);

        var previousWorkedDaysCount = await payrollRecordQuery.GetAnnualWorkedDaysCountAsync(
            workshop.UserId,
            employee.Id,
            periodStart,
            cancellationToken);

        var annualWorkedDaysCount = previousWorkedDaysCount + workInput.WorkedDaysCount;

        logger.LogInformation(
            "Annual context for employee {EmployeeId}: year has {DaysInYear} days and " +
            "{AnnualWorkedDaysCount} worked days in total ({PreviousWorkedDaysCount} persisted + {CurrentWorkedDaysCount} current)",
            employee.Id,
            daysInYear,
            annualWorkedDaysCount,
            previousWorkedDaysCount,
            workInput.WorkedDaysCount);

        // Every rule value active at the period start, loaded in a single
        // query and reused by all items and by the insurance/tax formulas.
        var ruleValues = await laborLawRuleQuery.GetActiveRuleValuesAsync(
            period.PeriodStart,
            cancellationToken);

        foreach (var ruleKey in Enum.GetValues<LaborLawRuleKey>())
        {
            if (!ruleValues.ContainsKey(ruleKey))
            {
                logger.LogWarning(
                    "Labor law rule {RuleKey} not found for period {PeriodStart}..{PeriodEnd}; it will not be available to formulas",
                    ruleKey,
                    period.PeriodStart,
                    period.PeriodEnd);
            }
        }

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
                ruleValues,
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

        var performanceBonusResult = AddOptionalAmount(workInput.PerformanceBonusAmount, "کارانه", employee.Id, cancellationToken);
        if (!performanceBonusResult.IsSuccess)
            return ConvertFailure(performanceBonusResult);

        var cashBenefitsResult = AddOptionalAmount(workInput.CashBenefitsAmount, "مزایای نقدی", employee.Id, cancellationToken);
        if (!cashBenefitsResult.IsSuccess)
            return ConvertFailure(cashBenefitsResult);

        var grossAmount = amounts.Values.Sum()
                          + (performanceBonusResult.Response ?? 0m)
                          + (cashBenefitsResult.Response ?? 0m);

        logger.LogInformation(
            "Gross amount calculated as {GrossAmount} for employee {EmployeeId}",
            grossAmount,
            employee.Id);

        var allRuleVariables = BuildRuleVariables(ruleValues);
        var itemVariables = BuildItemAmountVariables(amounts, performanceBonusResult.Response, cashBenefitsResult.Response);

        var insuranceResult = await CalculateInsuranceAmountAsync(
            itemVariables,
            allRuleVariables,
            employee.Id,
            period,
            cancellationToken);
        if (!insuranceResult.IsSuccess)
            return ConvertFailure(insuranceResult);

        var taxResult = await CalculateTaxAmountAsync(
            itemVariables,
            allRuleVariables,
            employee.Id,
            period,
            cancellationToken);
        if (!taxResult.IsSuccess)
            return ConvertFailure(taxResult);

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

    private static FormulaVariable[] BuildRuleVariables(
        IReadOnlyDictionary<LaborLawRuleKey, decimal> ruleValues)
    {
        var variables = new List<FormulaVariable>();

        foreach (var ruleKey in Enum.GetValues<LaborLawRuleKey>())
        {
            if (ruleValues.TryGetValue(ruleKey, out var ruleValue))
                variables.Add(new FormulaVariable(ruleKey.ToString(), ruleValue));
        }

        return variables.ToArray();
    }

    private static FormulaVariable[] BuildItemAmountVariables(
        IReadOnlyDictionary<FormulaKey, decimal> amounts,
        decimal? performanceBonusAmount,
        decimal? cashBenefitsAmount)
    {
        var variables = new List<FormulaVariable>();

        foreach (var key in Enum.GetValues<FormulaKey>())
        {
            if (key == FormulaKey.InsurancePay ||
                key == FormulaKey.TaxPay ||
                key == FormulaKey.TaxableAmountPay)
                continue;

            variables.Add(new FormulaVariable(key.ToString(), GetAmount(amounts, key)));
        }

        variables.Add(new FormulaVariable("PerformanceBonusAmount", performanceBonusAmount ?? 0m));
        variables.Add(new FormulaVariable("CashBenefitsAmount", cashBenefitsAmount ?? 0m));

        return variables.ToArray();
    }

    private async Task<Result<decimal?>> CalculateItemAsync(
        CalculationItem item,
        Employee employee,
        Workshop workshop,
        SalaryDecree salaryDecree,
        PayrollWorkInput workInput,
        PayrollPeriod period,
        int daysInYear,
        decimal annualWorkedDaysCount,
        IReadOnlyDictionary<LaborLawRuleKey, decimal> activeRuleValues,
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
            if (!activeRuleValues.TryGetValue(ruleKey, out var ruleValue))
            {
                logger.LogWarning(
                    "Labor law rule {RuleKey} was not found for {ItemName} of employee {EmployeeId} " +
                    "in period {PeriodStart}..{PeriodEnd}",
                    ruleKey,
                    item.DisplayName,
                    employee.Id,
                    period.PeriodStart,
                    period.PeriodEnd);

                return Result<decimal?>.NotfoundFailure(
                    $"قانون {ruleKey} برای محاسبه {item.DisplayName} یافت نشد.");
            }

            ruleValues.Add((ruleKey, ruleValue));
        }

        return await EvaluateFormulaAsync(
            item.FormulaKey,
            item.DisplayName,
            employee.Id,
            period,
            BuildEvaluationInputs(item, employee, workshop, salaryDecree, workInput, period, ruleValues, daysInYear, annualWorkedDaysCount),
            cancellationToken);
    }

    private static LaborLawRuleKey GetShiftWorkRuleKey(ShiftType shiftType) =>
        shiftType switch
        {
            ShiftType.MorningEvening => LaborLawRuleKey.ShiftWorkPercentageMorningEvening,
            ShiftType.MorningNight => LaborLawRuleKey.ShiftWorkPercentageMorningNight,
            ShiftType.EveningNight => LaborLawRuleKey.ShiftWorkPercentageEveningNight,
            ShiftType.MorningEveningNight => LaborLawRuleKey.ShiftWorkPercentageMorningEveningNight,
            _ => throw new ArgumentOutOfRangeException(nameof(shiftType), shiftType, null)
        };

    private static string GetRuleVariableName(FormulaKey formulaKey, LaborLawRuleKey ruleKey) =>
        formulaKey switch
        {
            FormulaKey.AnnualBonusPay => "AnnualBonusRuleAmount",
            FormulaKey.ShiftWorkPay => "ShiftWorkPercentage",
            _ => ruleKey.ToString()
        };

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
        FormulaVariable[] itemVariables,
        FormulaVariable[] ruleVariables,
        Guid employeeId,
        PayrollPeriod period,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting calculation of insurance amount for employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}",
            employeeId,
            period.PeriodStart,
            period.PeriodEnd);

        var inputs = itemVariables
            .Concat(ruleVariables)
            .Cast<object>()
            .ToArray();

        var insuranceResult = await EvaluateFormulaAsync(
            FormulaKey.InsurancePay,
            "بیمه",
            employeeId,
            period,
            inputs,
            cancellationToken);
        if (!insuranceResult.IsSuccess)
            return insuranceResult;

        logger.LogInformation(
            "Insurance amount calculated as {InsuranceAmount} for employee {EmployeeId}",
            insuranceResult.Response,
            employeeId);

        return insuranceResult;
    }

    private async Task<Result<decimal?>> CalculateTaxAmountAsync(
        FormulaVariable[] itemVariables,
        FormulaVariable[] ruleVariables,
        Guid employeeId,
        PayrollPeriod period,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting calculation of tax amount for employee {EmployeeId} in period {PeriodStart}..{PeriodEnd}",
            employeeId,
            period.PeriodStart,
            period.PeriodEnd);

        var taxableInputs = itemVariables
            .Concat(ruleVariables)
            .Cast<object>()
            .ToArray();

        var taxableAmountResult = await EvaluateFormulaAsync(
            FormulaKey.TaxableAmountPay,
            "مبلغ مشمول مالیات",
            employeeId,
            period,
            taxableInputs,
            cancellationToken);
        if (!taxableAmountResult.IsSuccess)
            return taxableAmountResult;

        var taxableAmount = taxableAmountResult.Response!.Value;

        var taxInputs = new List<object> { new FormulaVariable("TaxableAmount", taxableAmount) };
        taxInputs.AddRange(itemVariables);
        taxInputs.AddRange(ruleVariables);

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
        PayrollWorkInput workInput,
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