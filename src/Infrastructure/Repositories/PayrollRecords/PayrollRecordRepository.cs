namespace Infrastructure.Repositories.PayrollRecords;

public class PayrollRecordRepository : IPayrollRecordRepository
{
    public async Task<Guid?> CreateAsync(PayrollRecord payrollRecord, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PayrollRecord?> GetByIdAsync(Guid userId, Guid payrollRecordId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateAsync(PayrollRecord payrollRecord, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid payrollRecordId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}