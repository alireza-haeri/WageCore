namespace Infrastructure.Repositories.Employee;

public class EmployeeSalaryProfileRepository(
    WageCoreDbContext context,
    ILogger<EmployeeSalaryProfileRepository> logger)
    : IEmployeeSalaryProfileRepository
{
    public async Task<Guid?> CreateAsync(
        EmployeeSalaryProfile salaryProfile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.EmployeeSalaryProfiles.AddAsync(salaryProfile, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return salaryProfile.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while creating a salary profile for Employee: {EmployeeId}.",
                salaryProfile.EmployeeId);
            return null;
        }
    }

    public async Task<EmployeeSalaryProfile?> GetByIdAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken = default)
    {
        var userWorkshopIds = context.Workshops
            .Where(x => x.UserId == userId)
            .Select(x => x.Id);

        var userEmployeeIds = context.Employees
            .Where(x => userWorkshopIds.Contains(x.WorkshopId))
            .Select(x => x.Id);

        return await context.EmployeeSalaryProfiles
            .FirstOrDefaultAsync(
                x => x.Id == employeeSalaryProfileId && userEmployeeIds.Contains(x.EmployeeId),
                cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        EmployeeSalaryProfile salaryProfile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            context.EmployeeSalaryProfiles.Update(salaryProfile);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while updating a salary profile for Id: {SalaryProfileId}.",
                salaryProfile.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid employeeSalaryProfileId,
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

            var salaryProfile = await context.EmployeeSalaryProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == employeeSalaryProfileId && userEmployeeIds.Contains(x.EmployeeId),
                    cancellationToken);
            if (salaryProfile is null)
                return false;

            context.EmployeeSalaryProfiles.Remove(salaryProfile);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while deleting a salary profile for User: {UserId}.", userId);
            return false;
        }
    }
}
