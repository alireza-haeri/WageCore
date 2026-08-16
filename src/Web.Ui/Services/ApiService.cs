using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace Web.Ui.Services;

public interface IApiService
{
    Task<TResult?> SendAsync<TResult>(Func<Task<TResult>> apiCall, string? successMessage = null,
        bool showErrorToastValidation = true);
}

public class ApiService(IToastService toastService, NavigationManager navigationManager) : IApiService
{
    public async Task<TResult?> SendAsync<TResult>(
        Func<Task<TResult>> apiCall,
        string? successMessage = null,
        bool showErrorToastValidation = true)
    {
        try
        {
            var result = await apiCall();

            if (result == null)
            {
                if (showErrorToastValidation)
                    toastService.ShowToastError("پاسخی از سرور دریافت نشد!");
                return default;
            }

            var isSuccessProp = result.GetType().GetProperty("IsSuccess");
            var isSuccess = isSuccessProp != null && (bool?)isSuccessProp.GetValue(result) == true;

            if (isSuccess)
            {
                if (!string.IsNullOrEmpty(successMessage))
                    toastService.ShowToastSuccess(successMessage);
                return result;
            }

            if (showErrorToastValidation)
            {
                var errorsProp = result.GetType().GetProperty("Errors");
                if (errorsProp != null)
                    if (errorsProp.GetValue(result) is IDictionary<string, ICollection<string>> errors && errors.Any())
                        foreach (var errorGroup in errors)
                        foreach (var error in errorGroup.Value)
                            toastService.ShowToastError(error, errorGroup.Key);
                    else
                        toastService.ShowToastError("عملیات با خطا مواجه شد!");
                else
                    toastService.ShowToastError("عملیات با خطا مواجه شد!");
            }

            return result;
        }
        catch (ApiException ex)
        {
            var message = GetErrorMessage(ex.StatusCode, ex.Response);
            toastService.ShowToastError(message);

            return default;
        }
        catch (HttpRequestException)
        {
            toastService.ShowToastError("خطای شبکه! لطفاً اتصال اینترنت خود را بررسی کنید.");
            return default;
        }
        catch (JsonException)
        {
            toastService.ShowToastError("خطا در پردازش پاسخ سرور!");
            return default;
        }
        catch (Exception ex)
        {
            toastService.ShowToastError($"خطای غیرمنتظره: {ex.Message}");
            return default;
        }
    }

    private string GetErrorMessage(int statusCode, string? response)
    {
        try
        {
            if (!string.IsNullOrEmpty(response))
            {
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errorsElement))
                {
                    var errorMessages = new List<string>();
                    foreach (var property in errorsElement.EnumerateObject())
                    {
                        var fieldErrors = property.Value.EnumerateArray().Select(e => e.GetString());
                        errorMessages.AddRange(fieldErrors!);
                    }

                    return string.Join("\n", errorMessages);
                }

                if (root.TryGetProperty("title", out var titleElement))
                    return titleElement.GetString() ?? $"خطا ({statusCode})";

                if (root.TryGetProperty("message", out var messageElement))
                    return messageElement.GetString() ?? $"خطا ({statusCode})";
            }
        }
        catch
        {
            // ignored
        }

        return statusCode switch
        {
            400 => "درخواست نامعتبر! لطفاً اطلاعات را بررسی کنید.",
            401 => "شما اجازه دسترسی ندارید. لطفاً وارد شوید.",
            403 => "شما مجوز دسترسی به این بخش را ندارید.",
            404 => "منبع درخواستی یافت نشد!",
            409 => "اطلاعات تکراری! لطفاً دوباره تلاش کنید.",
            500 => "خطای سرور! لطفاً بعداً تلاش کنید.",
            _ => $"خطا ({statusCode}). لطفاً دوباره تلاش کنید."
        };
    }
}