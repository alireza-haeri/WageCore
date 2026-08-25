namespace Infrastructure.Repositories.Employee;

public class EmployeeRepository(WageCoreDbContext context, ILogger<EmployeeRepository> logger) : IEmployeeRepository
{
    public async Task<Guid?> CreateAsync(Core.Domain.Employee employee, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Employees.AddAsync(employee, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return employee.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while creating an employee for Workshop: {WorkshopId}.",
                employee.WorkshopId);
            return null;
        }
    }

    public async Task<Core.Domain.Employee?> GetByIdAsync(Guid userId, Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var userWorkshopIds = context.Workshops
            .Where(x => x.UserId == userId)
            .Select(x => x.Id);

        return await context.Employees
            .Include(x => x.BankAccounts)
            .FirstOrDefaultAsync(x => x.Id == employeeId && userWorkshopIds.Contains(x.WorkshopId), cancellationToken);
    }

    public async Task<bool> UpdateAsync(Core.Domain.Employee employee, CancellationToken cancellationToken = default)
    {
        try
        {
            context.Employees.Update(employee);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while updating an employee for Workshop: {WorkshopId}.",
                employee.WorkshopId);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userWorkshopIds = context.Workshops
                .Where(x => x.UserId == userId)
                .Select(x => x.Id);

            var employee = await context.Employees
                .Include(x => x.BankAccounts)
                .FirstOrDefaultAsync(x => x.Id == employeeId && userWorkshopIds.Contains(x.WorkshopId),
                    cancellationToken);
            if (employee is null)
                return false;

            context.Employees.Remove(employee);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while deleting an employee for User: {UserId}.", userId);
            return false;
        }
    }
}
