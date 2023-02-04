using BlazorBoilerplate.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace BlazorBoilerplate.Server.Hubs
{
    public class Notifier
    {
        private readonly IHubContext<MainHub, IHubClient> _hubContext;

        public Notifier(IHubContext<MainHub, IHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Raise(IReadOnlyList<string> users, Notification notification)
        {
            await _hubContext.Clients.Users(users).Notify(notification);
        }
    }
}