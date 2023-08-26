using BlazorBoilerplate.Shared.Models;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface INotifier
    {
        Task Raise(IReadOnlyList<string> users, Notification notification);
    }
}
