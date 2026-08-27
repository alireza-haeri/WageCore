namespace Core.Abstractions.Repositories;

public interface IUserRepository
{
    Task<IdentityResult> CreateAsync(User user,string password,CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<User?> GetAsync(string? phoneNumber, string? email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string? phoneNumber, string? email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}