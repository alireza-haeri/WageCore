namespace Application.Services;

public class PayrollLimitsResolver : IPayrollLimitsResolver
{
    public Task<Result<PayrollLimits>> ResolveAsync(
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
