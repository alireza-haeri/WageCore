namespace Application.Services;

public class PayrollCalculationService : IPayrollCalculationService
{
    public Result<PayrollCalculationResult> Calculate(
        Employee employee,
        Workshop workshop,
        IReadOnlyList<SalaryDecree> salaryProfiles,
        DateOnly periodStart,
        DateOnly periodEnd,
        PayrollWorkInputDto workInput)
    {
        throw new NotImplementedException();
    }
}
