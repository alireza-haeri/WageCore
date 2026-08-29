namespace Core.Abstractions.Services;

public interface IPayrollCalculationService
{
    Task<Result<PayrollCalculationResult>> CalculateAsync(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<EmployeeSalaryProfile> salaryProfiles,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInputDto workInput,
        CancellationToken cancellationToken = default);
}
