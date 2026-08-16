using Microsoft.AspNetCore.Components;

namespace Web.Ui.Services;

public interface IAuthenticationService
{
    Task LoginAsync(string token, int expiresInMinutes);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsAuthenticatedAsync();
}


public class AuthenticationService(IStorageService storage, NavigationManager navigationManager)
    : IAuthenticationService
{
    private readonly NavigationManager _navigationManager = navigationManager;

    private const string TokenKey = "authToken";
    private const string ExpirationKey = "authExpiration";

    public async Task LoginAsync(string token, int expiresInMinutes)
    {
        var expiration = DateTime.UtcNow.AddMinutes(expiresInMinutes);
        await storage.SetValueAsync(TokenKey, token);
        await storage.SetValueAsync(ExpirationKey, expiration);
    }

    public async Task LogoutAsync()
    {
        await storage.RemoveAsync(TokenKey);
        await storage.RemoveAsync(ExpirationKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await storage.GetValueAsync<string>(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var expiration = await storage.GetValueAsync<DateTime?>(ExpirationKey);
            if (expiration.HasValue && expiration.Value < DateTime.UtcNow)
            {
                await LogoutAsync();
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
}