namespace Infrastructure.Repositories.PayrollRecords;

public class PayrollRecordRepository(
    WageCoreDbContext context,
    ILogger<PayrollRecordRepository> logger)
    : IPayrollRecordRepository
{
    public async Task<Guid?> CreateAsync(
        PayrollRecord payrollRecord,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.PayrollRecords.AddAsync(payrollRecord, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return payrollRecord.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while creating a payroll record for Employee: {EmployeeId}.",
                payrollRecord.EmployeeId);
            return null;
        }
    }

    public async Task<PayrollRecord?> GetByIdAsync(
        Guid userId,
        Guid payrollRecordId,
        CancellationToken cancellationToken = default)
    {
        var userWorkshopIds = context.Workshops
            .Where(x => x.UserId == userId)
            .Select(x => x.Id);

        var userEmployeeIds = context.Employees
            .Where(x => userWorkshopIds.Contains(x.WorkshopId))
            .Select(x => x.Id);

        return await context.PayrollRecords
            .FirstOrDefaultAsync(
                x => x.Id == payrollRecordId && userEmployeeIds.Contains(x.EmployeeId),
                cancellationToken);
    }

    public async Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(
        Guid userId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        var userWorkshopIds = context.Workshops
            .Where(x => x.UserId == userId)
            .Select(x => x.Id);

        var userEmployeeIds = context.Employees
            .Where(x => userWorkshopIds.Contains(x.WorkshopId))
            .Select(x => x.Id);

        return await context.PayrollRecords
            .FirstOrDefaultAsync(
                x => x.EmployeeId == employeeId &&
                     x.PeriodStart == periodStart &&
                     x.PeriodEnd == periodEnd &&
                     userEmployeeIds.Contains(x.EmployeeId),
                cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        PayrollRecord payrollRecord,
        CancellationToken cancellationToken = default)
    {
        try
        {
            context.PayrollRecords.Update(payrollRecord);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while updating a payroll record for Id: {PayrollRecordId}.",
                payrollRecord.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid payrollRecordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userWorkshopIds = context.Workshops
                .Where(x => x.UserId == userId)
                .Select(x => x.Id);

            var userEmployeeIds = context.Employees
                .Where(x => userWorkshopIds.Contains(x.WorkshopId))
                .Select(x => x.Id);

            var payrollRecord = await context.PayrollRecords
                .FirstOrDefaultAsync(
                    x => x.Id == payrollRecordId && userEmployeeIds.Contains(x.EmployeeId),
                    cancellationToken);
            if (payrollRecord is null)
                return false;

            context.PayrollRecords.Remove(payrollRecord);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while deleting a payroll record for User: {UserId}.", userId);
            return false;
        }
    }
}
