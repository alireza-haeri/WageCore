namespace Core.Abstractions.Services;

public interface IPayrollCalculationService
{
    Task<Result<PayrollCalculationResult>> CalculateAsync(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<SalaryDecree> salaryDecrees,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInput workInput,
        CancellationToken cancellationToken = default);
}
