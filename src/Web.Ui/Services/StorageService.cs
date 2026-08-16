using Microsoft.JSInterop;

namespace Web.Ui.Services;

public interface IStorageService
{
    Task SetValueAsync<T>(string key, T value);
    Task<T?> GetValueAsync<T>(string key);
    Task RemoveAsync(string key);
    Task ClearAsync();
}

public class StorageService(IJSRuntime jsRuntime) : IStorageService
{
    public async Task SetValueAsync<T>(string key, T value)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public async Task<T?> GetValueAsync<T>(string key)
    {
        var data = await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(data))
            return default;

        return System.Text.Json.JsonSerializer.Deserialize<T>(data);
    }

    public async Task RemoveAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task ClearAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.clear");
    }
}