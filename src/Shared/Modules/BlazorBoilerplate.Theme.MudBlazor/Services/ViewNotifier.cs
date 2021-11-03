using BlazorBoilerplate.Shared.Interfaces;
using MudBlazor;

namespace BlazorBoilerplate.Theme.Material.Services
{
    public class ViewNotifier : IViewNotifier
    {
        private readonly ISnackbar snackbar;
        public ViewNotifier(ISnackbar snackbar)
        {
            this.snackbar = snackbar;
        }
        public void Show(string message, ViewNotifierType type, string title = null, string icon = null)
        {
            var snackType = Severity.Success;

            switch (type)
            {
                case ViewNotifierType.Error:
                    snackType = Severity.Error;
                    break;
                case ViewNotifierType.Warning:
                    snackType = Severity.Warning;
                    break;
                case ViewNotifierType.Info:
                    snackType = Severity.Info;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(title))
                message = $"<b>{title}</b><br />{message}";

            snackbar.Add(message, snackType, config =>
            {
                if (icon != null)
                    config.Icon = icon;
            });
        }
    }
}
