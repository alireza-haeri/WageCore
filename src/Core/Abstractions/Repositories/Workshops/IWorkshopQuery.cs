namespace Core.Abstractions.Repositories.Workshops;

public interface IWorkshopQuery
{
    Task<PagedResult<UserWorkshopResult>> GetUserWorkshopsAsync(Guid userId, PaginationDto pagination,
        string? searchName = null, WorkshopRegion? region = null, CancellationToken cancellationToken = default);

    Task<List<UserWorkshopNameResult>> GetUserWorkshopsNameAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserWorkshopByIdResult?> GetUserWorkshopByIdAsync(Guid userId, Guid workshopId, CancellationToken cancellationToken = default);
    Task<bool> IsExistWorkshopName(Guid userId,string workshopName, Guid? excludeWorkshopId = null, CancellationToken cancellationToken = default);
    Task<bool> IsExistWorkshopNationalId(Guid userId,string nationalId, Guid? excludeWorkshopId = null, CancellationToken cancellationToken = default);
}