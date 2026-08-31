namespace Core.Abstractions.Services;

public interface IPayrollCalculationService
{
    Result<PayrollCalculationResult> Calculate(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<EmployeeSalaryProfile> salaryProfiles,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInputDto workInput);
}
