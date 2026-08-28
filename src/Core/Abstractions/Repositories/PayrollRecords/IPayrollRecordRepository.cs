namespace Core.Abstractions.Repositories.PayrollRecords;

public interface IPayrollRecordRepository
{
    Task<Guid?> CreateAsync(PayrollRecord payrollRecord, CancellationToken cancellationToken = default);
}
