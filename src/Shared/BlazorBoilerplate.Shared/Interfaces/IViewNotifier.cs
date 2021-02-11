namespace BlazorBoilerplate.Shared.Interfaces
{
    public enum ViewNotifierType
    {
        Success,
        Info,
        Warning,
        Error
    }
    public interface IViewNotifier
    {
        void Show(string message, ViewNotifierType type, string title = null, string icon = null);
    }
}
