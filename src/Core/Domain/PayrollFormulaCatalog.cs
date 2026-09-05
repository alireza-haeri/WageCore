namespace Core.Domain;

/// <summary>
/// Reference payroll data: the labor-law rule values and calculation formula
/// expressions that a fresh installation needs before any payroll can be
/// calculated. The development seeder uses it to seed the database
/// automatically; the calculation service tests use it as their fixture.
/// </summary>
public static class PayrollFormulaCatalog
{
    public const decimal MinimumDailySalary = 2_000_000m;
    public const decimal MaximumOvertimeHoursPerMonth = 120m;
    public const decimal NightShiftHoursPerDay = 8m;
    public const decimal StandardDailyWorkHoursValue = 7.33m;
    public const decimal FridayWorkHoursPerDay = 16m;
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

    public static readonly IReadOnlyDictionary<LaborLawRuleKey, decimal> RuleValues =
        new Dictionary<LaborLawRuleKey, decimal>
        {
            [LaborLawRuleKey.MinimumDailySalary] = MinimumDailySalary,
            [LaborLawRuleKey.MaximumOvertimeHoursPerMonth] = MaximumOvertimeHoursPerMonth,
            [LaborLawRuleKey.NightShiftHoursPerDay] = NightShiftHoursPerDay,
            [LaborLawRuleKey.StandardDailyWorkHours] = StandardDailyWorkHoursValue,
            [LaborLawRuleKey.FridayWorkHoursPerDay] = FridayWorkHoursPerDay,
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
            [LaborLawRuleKey.AnnualBonusMaximumAmount] = AnnualBonusMaximumAmount
        };

    public static string Expression(FormulaKey key) => Formulas[key];

    public static readonly IReadOnlyDictionary<FormulaKey, string> Formulas =
        new Dictionary<FormulaKey, string>
        {
            [FormulaKey.BaseSalaryPay] =
                "[PayrollWorkInputWorkedDaysCount] * [SalaryDecreeBaseDailySalary]",

            [FormulaKey.AttractionAllowancePay] =
                "[SalaryDecreeAttractionAllowance]",

            [FormulaKey.SupervisionAllowancePay] =
                "[SalaryDecreeSupervisionAllowance]",

            [FormulaKey.NightShiftExtraPay] =
                "[PayrollWorkInputNightShiftHours] * ([NightShiftPercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.HolidayWorkPay] =
                "[PayrollWorkInputHolidayWorkHours] * ([HolidayWorkPercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.ChildAllowancePay] =
                "[SalaryDecreeChildrenCount] * [MinimumDailySalary] * [ChildAllowanceMultiplier]",

            [FormulaKey.HousingAllowancePay] =
                "([HousingAllowanceAmount] / [PayrollWorkInputStandardWorkingDaysCount]) * [PayrollWorkInputWorkedDaysCount]",

            [FormulaKey.FoodAllowancePay] =
                "[PayrollWorkInputWorkedDaysCount] * ([FoodAllowanceAmount] / [PayrollWorkInputStandardWorkingDaysCount])",

            [FormulaKey.MarriageAllowancePay] =
                "[MaritalStatus] = 1 ? [PayrollWorkInputWorkedDaysCount] * ([MarriageAllowanceAmount] / [PayrollWorkInputStandardWorkingDaysCount]) : 0",

            [FormulaKey.OvertimePay] =
                "[PayrollWorkInputOvertimeHours] * ([OvertimePercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.ShiftWorkPay] =
                "[PayrollWorkInputWorkedDaysCount] * ([ShiftWorkPercentage] / 100 * [SalaryDecreeBaseDailySalary])",

            [FormulaKey.DailyMissionPay] =
                "[PayrollWorkInputMissionDaysCount] * [SalaryDecreeBaseDailySalary]",

            [FormulaKey.FridayWorkPay] =
                "[PayrollWorkInputFridayWorkHours] * ([FridayWorkPercentage] / 100 * ([SalaryDecreeBaseDailySalary] / [StandardDailyWorkHours]))",

            [FormulaKey.EndOfServicePay] =
                "[SalaryDecreeBaseDailySalary] * ([EndOfServiceDaysPerYear] / [DaysInYear]) * [AnnualWorkedDaysCount]",

            [FormulaKey.AnnualBonusPay] =
                "[AnnualBonusRuleAmount] * [AnnualWorkedDaysCount] / [DaysInYear]",

            [FormulaKey.CommutingAllowancePay] =
                "[SalaryDecreeTransportationAllowanceNet]",

            [FormulaKey.InsurancePay] =
                "([BaseSalaryPay] + [AttractionAllowancePay] + [SupervisionAllowancePay] + " +
                "[NightShiftExtraPay] + [HolidayWorkPay] + [HousingAllowancePay] + " +
                "[FoodAllowancePay] + [MarriageAllowancePay] + [OvertimePay] + " +
                "[ShiftWorkPay] + [FridayWorkPay] + " +
                "[CommutingAllowancePay] + [PerformanceBonusAmount] + [CashBenefitsAmount]) " +
                "* [InsurancePercentage] / 100",

            [FormulaKey.TaxableAmountPay] =
                "[BaseSalaryPay] + [AttractionAllowancePay] + [SupervisionAllowancePay] + " +
                "[NightShiftExtraPay] + [HolidayWorkPay] + [ChildAllowancePay] + " +
                "[HousingAllowancePay] + [FoodAllowancePay] + [MarriageAllowancePay] + " +
                "[OvertimePay] + [ShiftWorkPay] + [FridayWorkPay] + [AnnualBonusPay] + " +
                "[CommutingAllowancePay] + [PerformanceBonusAmount] + [CashBenefitsAmount]",

            [FormulaKey.TaxPay] =
                "[TaxableAmount] <= 40000000 ? 0 : " +
                "([TaxableAmount] <= 80000000 ? ([TaxableAmount] - 40000000) * 10 / 100 : " +
                "([TaxableAmount] <= 100000000 ? ((80000000 - 40000000) * 10 / 100) + ([TaxableAmount] - 80000000) * 15 / 100 : " +
                "([TaxableAmount] <= 120000000 ? ((80000000 - 40000000) * 10 / 100) + ((100000000 - 80000000) * 15 / 100) + ([TaxableAmount] - 100000000) * 20 / 100 : " +
                "([TaxableAmount] <= 140000000 ? ((80000000 - 40000000) * 10 / 100) + ((100000000 - 80000000) * 15 / 100) + ((120000000 - 100000000) * 20 / 100) + ([TaxableAmount] - 120000000) * 25 / 100 : " +
                "((80000000 - 40000000) * 10 / 100) + ((100000000 - 80000000) * 15 / 100) + ((120000000 - 100000000) * 20 / 100) + ((140000000 - 120000000) * 25 / 100) + ([TaxableAmount] - 140000000) * 30 / 100))))"
        };
}
