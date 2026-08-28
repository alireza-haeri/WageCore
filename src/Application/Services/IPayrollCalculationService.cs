namespace Application.Services;

public interface IPayrollCalculationService
{
    Task<Result<PayrollCalculationResult>> CalculateAsync(
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInputDto workInput,
        CancellationToken cancellationToken = default);
}
