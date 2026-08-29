using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PayrollCalculationService(
    ILaborLawRuleQuery laborLawRuleQuery,
    ILogger<PayrollCalculationService> logger)
    : IPayrollCalculationService
{
    private const decimal PercentBase = 100m;

    public async Task<Result<PayrollCalculationResult>> CalculateAsync(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<EmployeeSalaryProfile> salaryProfiles,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInputDto workInput,
        CancellationToken cancellationToken = default)
    {
        var salaryProfile = GetSalaryProfileInForce(salaryProfiles, periodStart);
        if (salaryProfile is null)
            return Result<PayrollCalculationResult>.NotfoundFailure("برای این بازه حکم حقوقی کارمند یافت نشد.");

        var rulesResult = await GetActiveRulesAsync(periodStart, cancellationToken);
        if (!rulesResult.IsSuccess)
            return Result<PayrollCalculationResult>.NotfoundFailure(rulesResult.Errors!["General"][0]);

        var rules = rulesResult.Response!;
        var hourlyWage = salaryProfile.BaseMonthlySalary / rules.MonthlyWorkingHours;
        var package = GetMonthlyPackage(salaryProfile, employee);
        var paidDaysRatio = GetPaidDaysRatio(workInput, periodStart, periodEnd);
        var periodBaseAmount = Round(package * paidDaysRatio);
        var overtimeAmount = Round(
            hourlyWage *
            GetAmount(workInput.OvertimeHours) *
            (PercentBase + rules.OvertimePremiumPercent) /
            PercentBase);
        var nightShiftExtraAmount = Round(
            hourlyWage *
            GetAmount(workInput.NightShiftHours) *
            rules.NightShiftExtraPercent /
            PercentBase);
        var fridayWorkAllowance = Round(
            hourlyWage *
            GetAmount(workInput.FridayWorkHours) *
            rules.FridayWorkPercent /
            PercentBase);

        var grossAmount = periodBaseAmount + overtimeAmount + nightShiftExtraAmount + fridayWorkAllowance;
        var calculatedTaxAmount = employee.IsTaxSubject
            ? Round(grossAmount * rules.TaxPercent / PercentBase)
            : 0m;

        return Result<PayrollCalculationResult>.Success(new PayrollCalculationResult(
            rules.MaximumMonthlyOvertimeHours,
            rules.MaximumFridayWorkHours,
            overtimeAmount,
            nightShiftExtraAmount,
            fridayWorkAllowance,
            calculatedTaxAmount,
            Round(grossAmount - calculatedTaxAmount)));
    }

    private async Task<Result<PayrollRules>> GetActiveRulesAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var monthlyWorkingHours = await GetRuleValueAsync(LaborLawRuleKey.MonthlyWorkingHours, date, cancellationToken);
        if (monthlyWorkingHours is null or 0)
            return Result<PayrollRules>.NotfoundFailure("ساعات کار ماهانه یافت نشد.");

        var overtimePremiumPercent = await GetRuleValueAsync(
            LaborLawRuleKey.OvertimePremiumPercent,
            date,
            cancellationToken);
        if (overtimePremiumPercent is null)
            return Result<PayrollRules>.NotfoundFailure("درصد اضافه‌کاری یافت نشد.");

        var nightShiftExtraPercent = await GetRuleValueAsync(
            LaborLawRuleKey.NightShiftExtraPercent,
            date,
            cancellationToken);
        if (nightShiftExtraPercent is null)
            return Result<PayrollRules>.NotfoundFailure("درصد فوق‌العاده شیفت شب یافت نشد.");

        var fridayWorkPercent = await GetRuleValueAsync(LaborLawRuleKey.FridayWorkPercent, date, cancellationToken);
        if (fridayWorkPercent is null)
            return Result<PayrollRules>.NotfoundFailure("درصد حق کار جمعه یافت نشد.");

        var taxPercent = await GetRuleValueAsync(LaborLawRuleKey.TaxPercent, date, cancellationToken);
        if (taxPercent is null)
            return Result<PayrollRules>.NotfoundFailure("نرخ مالیات یافت نشد.");

        var maximumMonthlyOvertimeHours = await GetRuleValueAsync(
            LaborLawRuleKey.MaximumMonthlyOvertimeHours,
            date,
            cancellationToken);
        if (maximumMonthlyOvertimeHours is null)
            return Result<PayrollRules>.NotfoundFailure("حداکثر ساعات اضافه‌کاری ماهانه یافت نشد.");

        var maximumFridayWorkHours = await GetRuleValueAsync(
            LaborLawRuleKey.MaximumFridayWorkHours,
            date,
            cancellationToken);
        if (maximumFridayWorkHours is null)
            return Result<PayrollRules>.NotfoundFailure("حداکثر ساعات کار جمعه یافت نشد.");

        return Result<PayrollRules>.Success(new PayrollRules(
            monthlyWorkingHours.Value,
            overtimePremiumPercent.Value,
            nightShiftExtraPercent.Value,
            fridayWorkPercent.Value,
            taxPercent.Value,
            maximumMonthlyOvertimeHours.Value,
            maximumFridayWorkHours.Value));
    }

    private async Task<decimal?> GetRuleValueAsync(
        LaborLawRuleKey key,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var value = await laborLawRuleQuery.GetActiveValueAsync(key, date, cancellationToken);
        if (value is null)
            logger.LogCritical("Labor law rule {RuleKey} for {Date} is not configured", key, date);

        return value;
    }

    private static EmployeeSalaryProfile? GetSalaryProfileInForce(
        IReadOnlyList<EmployeeSalaryProfile> salaryProfiles,
        DateOnly periodStart) =>
        salaryProfiles
            .Where(x => x.EffectiveFrom <= periodStart)
            .MaxBy(x => x.EffectiveFrom) ??
        salaryProfiles
            .MinBy(x => x.EffectiveFrom);

    private static decimal GetMonthlyPackage(EmployeeSalaryProfile salaryProfile, Employee employee) =>
        salaryProfile.BaseMonthlySalary +
        salaryProfile.HousingAllowance.GetValueOrDefault() +
        salaryProfile.FoodAllowance.GetValueOrDefault() +
        salaryProfile.ChildAllowancePerChild.GetValueOrDefault() * employee.ChildrenCount +
        salaryProfile.TransportationAllowanceNet.GetValueOrDefault() +
        salaryProfile.KaranehAmountNet.GetValueOrDefault() +
        salaryProfile.AttractionAllowance.GetValueOrDefault() +
        salaryProfile.SupervisionAllowance.GetValueOrDefault();

    private static decimal GetPaidDaysRatio(
        PayrollWorkInputDto workInput,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        var periodDays = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var paidDays = GetAmount(workInput.WorkedDaysCount) +
                       GetAmount(workInput.LeaveDaysCount) +
                       GetAmount(workInput.MissionDaysCount);

        return paidDays / periodDays;
    }

    private static decimal GetAmount(decimal? amount) =>
        Math.Max(amount.GetValueOrDefault(), 0m);

    private static decimal Round(decimal amount) =>
        Math.Round(amount, 0, MidpointRounding.AwayFromZero);

    private sealed record PayrollRules(
        decimal MonthlyWorkingHours,
        decimal OvertimePremiumPercent,
        decimal NightShiftExtraPercent,
        decimal FridayWorkPercent,
        decimal TaxPercent,
        decimal MaximumMonthlyOvertimeHours,
        decimal MaximumFridayWorkHours);
}
