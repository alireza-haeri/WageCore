namespace Core.Abstractions.Services;

public interface IPayrollCalculationService
{
    Result<PayrollCalculationResult> Calculate(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<SalaryDecree> salaryProfiles,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInputDto workInput);
}
