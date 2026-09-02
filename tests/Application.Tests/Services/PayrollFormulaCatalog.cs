namespace Application.Tests.Services;

// Canonical payroll formula expressions and the initial labor law rule values
// they consume, mirroring the seed data a site manager enters through the
// CalculationFormulas/LaborLawRules screens. The formulas are stored per key and
// resolved at run time by ICalculationFormulaQuery; the constants below let the
// tests evaluate the exact production expressions against the real evaluator so
// a mismatch between an expression and the variables PayrollCalculationService
// passes fails the suite instead of surfacing at runtime.
//
// Guiding rule: expressions contain no business coefficient, percentage,
// multiplier, threshold or rate as a literal — every such number arrives as a
// [LaborLawRuleKey-named] FormulaVariable. The only literals allowed are "100"
// (percent-to-fraction conversion) and "1"/"0" used as the MaritalStatus
// discriminant (Single=0, Married=1) in the marriage allowance expression.
//
// Work-input variables are prefixed with the full model type name
// ("PayrollWorkInputDtoWorkedDaysCount") because the evaluator binds model
// properties as {ModelTypeName}{PropertyName}.
internal static class PayrollFormulaCatalog
{
    // --- Initial rule values (seed data the site manager enters per key) ---
    public const decimal MinimumDailySalary = 2_000_000m;
    public const decimal StandardDailyWorkHoursValue = 7.33m; // 44-hour week over 6 working days
    public const decimal NightShiftPercentage = 35m;
    public const decimal HolidayWorkPercentage = 180m;
    public const decimal OvertimePercentage = 140m;
    public const decimal FridayWorkPercentage = 180m;
    public const decimal ChildAllowanceMultiplier = 3m;
    public const decimal EndOfServiceDaysPerYear = 30m;
    public const decimal HousingAllowanceAmount = 2_000_000m;
    public const decimal FoodAllowanceAmount = 1_400_000m;
    public const decimal MarriageAllowanceAmount = 2_000_000m;
    public const decimal InsurancePercentage = 7m;
    public const decimal AnnualBonusMinimumAmount = 3_000_000m;
    public const decimal AnnualBonusMaximumAmount = 6_000_000m;
    public const decimal ShiftWorkPercentageMorningEvening = 10m;
    public const decimal ShiftWorkPercentageMorningNight = 15m;
    public const decimal ShiftWorkPercentageEveningNight = 22.5m;
    public const decimal ShiftWorkPercentageMorningEveningNight = 22.5m;
    public const decimal TaxBracket1Threshold = 40_000_000m;
    public const decimal TaxBracket2Threshold = 80_000_000m;
    public const decimal TaxBracket2Rate = 10m;
    public const decimal TaxBracket3Threshold = 100_000_000m;
    public const decimal TaxBracket3Rate = 15m;
    public const decimal TaxBracket4Threshold = 120_000_000m;
    public const decimal TaxBracket4Rate = 20m;
    public const decimal TaxBracket5Threshold = 140_000_000m;
    public const decimal TaxBracket5Rate = 25m;
    public const decimal TaxBracket6Rate = 30m;

    public static readonly IReadOnlyDictionary<LaborLawRuleKey, decimal> RuleValues =
        new Dictionary<LaborLawRuleKey, decimal>
        {
            [LaborLawRuleKey.MinimumDailySalary] = MinimumDailySalary,
            [LaborLawRuleKey.StandardDailyWorkHours] = StandardDailyWorkHoursValue,
            [LaborLawRuleKey.NightShiftPercentage] = NightShiftPercentage,
            [LaborLawRuleKey.HolidayWorkPercentage] = HolidayWorkPercentage,
            [LaborLawRuleKey.OvertimePercentage] = OvertimePercentage,
            [LaborLawRuleKey.FridayWorkPercentage] = FridayWorkPercentage,
            [LaborLawRuleKey.ChildAllowanceMultiplier] = ChildAllowanceMultiplier,
            [LaborLawRuleKey.EndOfServiceDaysPerYear] = EndOfServiceDaysPerYear,
            [LaborLawRuleKey.HousingAllowanceAmount] = HousingAllowanceAmount,
            [LaborLawRuleKey.FoodAllowanceAmount] = FoodAllowanceAmount,
            [LaborLawRuleKey.MarriageAllowanceAmount] = MarriageAllowanceAmount,
            [LaborLawRuleKey.ShiftWorkPercentageMorningEvening] = ShiftWorkPercentageMorningEvening,
            [LaborLawRuleKey.ShiftWorkPercentageMorningNight] = ShiftWorkPercentageMorningNight,
            [LaborLawRuleKey.ShiftWorkPercentageEveningNight] = ShiftWorkPercentageEveningNight,
            [LaborLawRuleKey.ShiftWorkPercentageMorningEveningNight] = ShiftWorkPercentageMorningEveningNight,
            [LaborLawRuleKey.InsurancePercentage] = InsurancePercentage,
            [LaborLawRuleKey.AnnualBonusMinimumAmount] = AnnualBonusMinimumAmount,
            [LaborLawRuleKey.AnnualBonusMaximumAmount] = AnnualBonusMaximumAmount,
            [LaborLawRuleKey.TaxBracket1Threshold] = TaxBracket1Threshold,
            [LaborLawRuleKey.TaxBracket2Threshold] = TaxBracket2Threshold,
            [LaborLawRuleKey.TaxBracket2Rate] = TaxBracket2Rate,
            [LaborLawRuleKey.TaxBracket3Threshold] = TaxBracket3Threshold,
            [LaborLawRuleKey.TaxBracket3Rate] = TaxBracket3Rate,
            [LaborLawRuleKey.TaxBracket4Threshold] = TaxBracket4Threshold,
            [LaborLawRuleKey.TaxBracket4Rate] = TaxBracket4Rate,
            [LaborLawRuleKey.TaxBracket5Threshold] = TaxBracket5Threshold,
            [LaborLawRuleKey.TaxBracket5Rate] = TaxBracket5Rate,
            [LaborLawRuleKey.TaxBracket6Rate] = TaxBracket6Rate
        };

    // --- Canonical expressions (data entered per FormulaKey) ---
    public static string Expression(FormulaKey key) => Formulas[key];

    public static readonly IReadOnlyDictionary<FormulaKey, string> Formulas =
        new Dictionary<FormulaKey, string>
        {
            [FormulaKey.BaseSalaryPay] =
                "[PayrollWorkInputDtoWorkedDaysCount] * [SalaryDecreeBaseDailySalary]",

            [FormulaKey.AttractionAllowancePay] =
                "[SalaryDecreeAttractionAllowance]",

            [FormulaKey.SupervisionAllowancePay] =
                "[SalaryDecreeSupervisionAllowance]",

            [FormulaKey.NightShiftExtraPay] =
                "[PayrollWorkInputDtoNightShiftHours] * ([NightShiftPercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.HolidayWorkPay] =
                "[PayrollWorkInputDtoHolidayWorkHours] * ([HolidayWorkPercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.ChildAllowancePay] =
                "[SalaryDecreeChildrenCount] * [MinimumDailySalary] * [ChildAllowanceMultiplier]",

            [FormulaKey.HousingAllowancePay] =
                "([HousingAllowanceAmount] / [PayrollWorkInputDtoStandardWorkingDaysCount]) * [PayrollWorkInputDtoWorkedDaysCount]",

            [FormulaKey.FoodAllowancePay] =
                "[PayrollWorkInputDtoWorkedDaysCount] * ([FoodAllowanceAmount] / [PayrollWorkInputDtoStandardWorkingDaysCount])",

            [FormulaKey.MarriageAllowancePay] =
                "[MaritalStatus] = 1 ? [PayrollWorkInputDtoWorkedDaysCount] * ([MarriageAllowanceAmount] / [PayrollWorkInputDtoStandardWorkingDaysCount]) : 0",

            [FormulaKey.OvertimePay] =
                "[PayrollWorkInputDtoOvertimeHours] * ([OvertimePercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.ShiftWorkPay] =
                "[PayrollWorkInputDtoWorkedDaysCount] * ([ShiftWorkPercentage] / 100 * [SalaryDecreeBaseDailySalary])",

            [FormulaKey.DailyMissionPay] =
                "[PayrollWorkInputDtoMissionDaysCount] * [SalaryDecreeBaseDailySalary]",

            [FormulaKey.FridayWorkPay] =
                "[PayrollWorkInputDtoFridayWorkHours] * ([FridayWorkPercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.EndOfServicePay] =
                "[SalaryDecreeBaseDailySalary] * ([EndOfServiceDaysPerYear] / [DaysInYear]) * [AnnualWorkedDaysCount]",

            [FormulaKey.AnnualBonusPay] =
                "[AnnualBonusRuleAmount] * [AnnualWorkedDaysCount] / [DaysInYear]",

            // Commuting is paid from the transportation allowance set on the
            // decree (SalaryDecreeTransportationAllowanceNet) rather than from a
            // labor law rule, so the expression has no rule-driven variable.
            [FormulaKey.CommutingAllowancePay] =
                "[SalaryDecreeTransportationAllowanceNet]",

            [FormulaKey.InsurancePay] =
                "[GrossAmount] * [InsurancePercentage] / 100",

            // TODO: the taxable set (everything except DailyMissionPay and
            // EndOfServicePay) is a first pass and may need refinement later;
            // adjust this expression when the tax-exempt components change.
            [FormulaKey.TaxableAmountPay] =
                "[BaseSalaryPay] + [AttractionAllowancePay] + [SupervisionAllowancePay] + " +
                "[NightShiftExtraPay] + [HolidayWorkPay] + [ChildAllowancePay] + " +
                "[HousingAllowancePay] + [FoodAllowancePay] + [MarriageAllowancePay] + " +
                "[OvertimePay] + [ShiftWorkPay] + [FridayWorkPay] + [AnnualBonusPay] + " +
                "[CommutingAllowancePay] + [PerformanceBonusAmount] + [CashBenefitsAmount]",

            // TODO: known limitation — the rule-per-bracket approach fixes the
            // bracket count in this expression; adding a 7th bracket still
            // requires a formula text change, not only a new rule row.
            [FormulaKey.TaxPay] =
                "[TaxableAmount] <= [TaxBracket1Threshold] ? 0 : " +
                "([TaxableAmount] <= [TaxBracket2Threshold] ? ([TaxableAmount] - [TaxBracket1Threshold]) * [TaxBracket2Rate] / 100 : " +
                "([TaxableAmount] <= [TaxBracket3Threshold] ? (([TaxBracket2Threshold] - [TaxBracket1Threshold]) * [TaxBracket2Rate] / 100) + ([TaxableAmount] - [TaxBracket2Threshold]) * [TaxBracket3Rate] / 100 : " +
                "([TaxableAmount] <= [TaxBracket4Threshold] ? (([TaxBracket2Threshold] - [TaxBracket1Threshold]) * [TaxBracket2Rate] / 100) + (([TaxBracket3Threshold] - [TaxBracket2Threshold]) * [TaxBracket3Rate] / 100) + ([TaxableAmount] - [TaxBracket3Threshold]) * [TaxBracket4Rate] / 100 : " +
                "([TaxableAmount] <= [TaxBracket5Threshold] ? (([TaxBracket2Threshold] - [TaxBracket1Threshold]) * [TaxBracket2Rate] / 100) + (([TaxBracket3Threshold] - [TaxBracket2Threshold]) * [TaxBracket3Rate] / 100) + (([TaxBracket4Threshold] - [TaxBracket3Threshold]) * [TaxBracket4Rate] / 100) + ([TaxableAmount] - [TaxBracket4Threshold]) * [TaxBracket5Rate] / 100 : " +
                "(([TaxBracket2Threshold] - [TaxBracket1Threshold]) * [TaxBracket2Rate] / 100) + (([TaxBracket3Threshold] - [TaxBracket2Threshold]) * [TaxBracket3Rate] / 100) + (([TaxBracket4Threshold] - [TaxBracket3Threshold]) * [TaxBracket4Rate] / 100) + (([TaxBracket5Threshold] - [TaxBracket4Threshold]) * [TaxBracket5Rate] / 100) + ([TaxableAmount] - [TaxBracket5Threshold]) * [TaxBracket6Rate] / 100))))"
        };
}
