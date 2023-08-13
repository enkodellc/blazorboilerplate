using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Server.Extensions;
using BlazorBoilerplate.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BlazorBoilerplate.Server.Hubs
{
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    public class MainHub : Hub<IHubClient>
    {
        private const string AuthSchemes =
            "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme; //Cookie + Token authentication

        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<MainHub> _logger;

        public static readonly ConcurrentDictionary<string, Guid> OnlineUsers = new();

        public MainHub(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            ILogger<MainHub> logger)
        {
            _context = context;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task NotifyOnlineUsers(Guid userId, bool online)
        {
            var users = await _context.GetUserManagers();

            await Clients.Users(users).NotifyOnlineUsers(userId, online);
        }

        public async Task<IEnumerable<Guid>> GetOnlineUsers()
        {
            var onlineUsers = OnlineUsers.Values.Distinct();

            if (!(await _authorizationService.AuthorizeAsync(Context.User, Policies.For(UserFeatures.UserManager))).Succeeded)
                onlineUsers = null;

            return onlineUsers;
        }

        public async override Task OnConnectedAsync()
        {
            var clientId = Context.User.GetClientId();

            if (clientId == null)
            {
                OnlineUsers.AddOrUpdate(Context.ConnectionId, Context.User.GetUserId(), (key, oldValue) => oldValue);

                _logger.LogDebug($"User {Context.User.Identity.Name} ({Context.ConnectionId}) connected to MainHub");

                await NotifyOnlineUsers(Context.User.GetUserId(), true);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            var clientId = Context.User.GetClientId();

            if (clientId == null)
            {
                OnlineUsers.Remove(Context.ConnectionId, out Guid userId);

                _logger.LogDebug($"User {Context.User.Identity.Name} ({Context.ConnectionId}) disconnected from MainHub");

                await NotifyOnlineUsers(userId, false);
            }

            await base.OnDisconnectedAsync(e);
        }
    }
}