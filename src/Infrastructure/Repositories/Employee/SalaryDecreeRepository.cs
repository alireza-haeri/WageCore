namespace Infrastructure.Repositories.Employee;

public class SalaryDecreeRepository(
    WageCoreDbContext context,
    ILogger<SalaryDecreeRepository> logger)
    : ISalaryDecreeRepository
{
    public async Task<Guid?> CreateAsync(
        SalaryDecree salaryProfile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SalaryDecrees.AddAsync(salaryProfile, cancellationToken);
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

    public async Task<SalaryDecree?> GetByIdAsync(
        Guid userId,
        Guid salaryDecreeId,
        CancellationToken cancellationToken = default)
    {
        var userWorkshopIds = context.Workshops
            .Where(x => x.UserId == userId)
            .Select(x => x.Id);

        var userEmployeeIds = context.Employees
            .Where(x => userWorkshopIds.Contains(x.WorkshopId))
            .Select(x => x.Id);

        return await context.SalaryDecrees
            .FirstOrDefaultAsync(
                x => x.Id == salaryDecreeId && userEmployeeIds.Contains(x.EmployeeId),
                cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        SalaryDecree salaryProfile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            context.SalaryDecrees.Update(salaryProfile);
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
        Guid salaryDecreeId,
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

            var salaryProfile = await context.SalaryDecrees
                .FirstOrDefaultAsync(
                    x => x.Id == salaryDecreeId && userEmployeeIds.Contains(x.EmployeeId),
                    cancellationToken);
            if (salaryProfile is null)
                return false;

            context.SalaryDecrees.Remove(salaryProfile);
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
