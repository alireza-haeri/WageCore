namespace Core.Abstractions.Repositories.PayrollRecords;

public interface IPayrollRecordRepository
{
    Task<Guid?> CreateAsync(PayrollRecord payrollRecord, CancellationToken cancellationToken = default);

    Task<PayrollRecord?> GetByIdAsync(
        Guid userId,
        Guid payrollRecordId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(PayrollRecord payrollRecord, CancellationToken cancellationToken = default);
}
