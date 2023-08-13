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

        public async Task NotifyLongOperationCompleted(IReadOnlyList<string> users, string message, bool success)
        {
            await _hubContext.Clients.Users(users).NotifyLongOperationCompleted(message, success);
        }
    }
}