using MudBlazor;

namespace LMS.Web.Client.Services
{
    public class ClientToastService : ToastService
    {
        private readonly ISnackbar _snackbar;

        public ClientToastService(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            _snackbar.Add(message, Severity.Success);
        }

        public void ShowInfo(string message, string title = "Info")
        {
            _snackbar.Add(message, Severity.Info);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            _snackbar.Add(message, Severity.Warning);
        }

        public void ShowError(string message, string title = "Error")
        {
            _snackbar.Add(message, Severity.Error);
        }
    }
}