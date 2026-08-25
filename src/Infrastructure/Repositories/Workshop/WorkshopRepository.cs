namespace Infrastructure.Repositories.Workshop;

public class WorkshopRepository(WageCoreDbContext context, ILogger<WorkshopRepository> logger) : IWorkShopRepository
{
    public async Task<Guid?> CreateAsync(Core.Domain.Workshop workshop, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Workshops.AddAsync(workshop, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return workshop.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while creating a workshop for User: {UserId}.", workshop.UserId);
            return null;
        }
    }

    public async Task<Core.Domain.Workshop?> GetByIdAsync(Guid userId, Guid workshopId,
        CancellationToken cancellationToken = default)
    {
        return await context.Workshops
            .Include(w => w.Departments)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Id == workshopId, cancellationToken);
    }

    public async Task<Core.Domain.Workshop?> GetByDepartmentIdAsync(Guid userId, Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        return await context.Workshops
            .Include(w => w.Departments)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Departments.Any(d => d.Id == departmentId),
                cancellationToken);
    }

    public async Task<bool> UpdateAsync(Core.Domain.Workshop workshop, CancellationToken cancellationToken = default)
    {
        try
        {
            context.Workshops.Update(workshop);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while updating a workshop for User: {UserId}.", workshop.UserId);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid workshopId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workshop = await context.Workshops
                .Include(w => w.Departments)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Id == workshopId, cancellationToken);
            if (workshop == null)
                return false;

            context.Workshops.Remove(workshop);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while deleting a workshop for User: {UserId}.", userId);
            return false;
        }
    }
}