using BlazorBoilerplate.Shared.Models;

namespace BlazorBoilerplate.Server.Hubs
{
    public interface IHubClient
    {
        Task Notify(Notification notification, string sender = null);

        Task NotifyOnlineUsers(Guid userId, bool online);
    }
}
