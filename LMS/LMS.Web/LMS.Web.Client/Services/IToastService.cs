namespace LMS.Web.Client.Services
{
    public interface ToastService
    {
        void ShowSuccess(string message, string title = "Success");
        void ShowInfo(string message, string title = "Info");
        void ShowWarning(string message, string title = "Warning");
        void ShowError(string message, string title = "Error");
    }
}