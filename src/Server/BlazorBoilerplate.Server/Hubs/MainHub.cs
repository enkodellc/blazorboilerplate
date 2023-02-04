using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorBoilerplate.Server.Hubs
{
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    public class MainHub : Hub<IHubClient>
    {
        private const string AuthSchemes =
            "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme; //Cookie + Token authentication

        private readonly ILogger<MainHub> _logger;

        public MainHub(
            ILogger<MainHub> logger)
        {
            _logger = logger;
        }

        public async override Task OnConnectedAsync()
        {
            _logger.LogDebug($"User {Context.User.Identity.Name} ({Context.ConnectionId}) connected to MainHub");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            _logger.LogDebug($"User {Context.User.Identity.Name} ({Context.ConnectionId}) disconnected from MainHub");

            await base.OnDisconnectedAsync(e);
        }
    }
}