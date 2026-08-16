using Microsoft.JSInterop;

namespace Web.Ui.Services;

public interface IToastService
{
    void ShowToastInfo(string message, string title = "نکته");
    void ShowToastSuccess(string message, string title = "موفقیت");
    void ShowToastError(string message, string title = "خطا");
    void ShowToastWarning(string message, string title = "هشدار");
}

public class ToastService(IJSRuntime jsRuntime) : IToastService
{
    public void ShowToastInfo(string message, string title = "نکته")
        => jsRuntime.InvokeAsync<Task>("toast", message);

    public void ShowToastSuccess(string message, string title = "موفقیت")
        => jsRuntime.InvokeAsync<Task>("toast", message);

    public void ShowToastError(string message, string title = "خطا")
        => jsRuntime.InvokeAsync<Task>("toast", message);

    public void ShowToastWarning(string message, string title = "هشدار")
        => jsRuntime.InvokeAsync<Task>("toast", message);
}