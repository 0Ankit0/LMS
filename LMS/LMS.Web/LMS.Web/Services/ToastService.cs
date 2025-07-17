using Microsoft.AspNetCore.Components;

namespace LMS.Web.Services;

public class ToastService
{
    public event Action<string, string, string>? OnShow;

    public void ShowSuccess(string title, string message)
    {
        OnShow?.Invoke("success", title, message);
    }

    public void ShowError(string title, string message)
    {
        OnShow?.Invoke("danger", title, message);
    }

    public void ShowWarning(string title, string message)
    {
        OnShow?.Invoke("warning", title, message);
    }

    public void ShowInfo(string title, string message)
    {
        OnShow?.Invoke("info", title, message);
    }
}
