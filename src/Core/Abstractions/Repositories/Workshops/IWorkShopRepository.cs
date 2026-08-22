namespace Core.Abstractions.Repositories.Workshops;

public interface IWorkShopRepository
{
    Task<Guid?> CreateAsync(Workshop workshop, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByIdAsync(Guid userId, Guid workshopId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Workshop workshop, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, Guid workshopId, CancellationToken cancellationToken = default);
}