namespace Core.Abstractions.Services;

public interface IPayrollLimitsResolver
{
    Task<Result<PayrollLimits>> ResolveAsync(
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);
}
