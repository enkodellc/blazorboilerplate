using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Storage;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BlazorBoilerplate.Server.Hubs
{
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    public class ChatHub : Hub
    {
        private const string AuthSchemes =
            "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme; //Cookie + Token authentication

        private static readonly Dictionary<string, string> userLookup = new();

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ILogger<ChatHub> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteMessage(int id)
        {
            try
            {
                var message = await _dbContext.Messages.SingleOrDefaultAsync(i => i.Id == id);

                if (message != null)
                {
                    _dbContext.Messages.Remove(message);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogDebug($"DeleteMessage '{message.Text}' from '{message.Sender}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteMessage Failed: {ex.GetBaseException().Message}");
            }
        }

        /// <summary>
        /// Send a message to all clients
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string message)
        {
            try
            {
                var subClaim = Context.User.Claims.Where(c => c.Type == JwtClaimTypes.Subject).SingleOrDefault();

                var newMessage = new Message()
                {
                    Text = message,
                    When = DateTime.UtcNow
                };

                if (subClaim != null)
                {
                    var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

                    newMessage.UserName = user.UserName;
                    newMessage.UserID = user.Id;

                    _dbContext.Messages.Add(newMessage);

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    var clientIdClaim = Context.User.Claims.Where(c => c.Type == JwtClaimTypes.ClientId).SingleOrDefault();

                    if (clientIdClaim != null)
                        newMessage.UserName = clientIdClaim.Value;
                }

                await Clients.All.SendAsync("ReceiveMessage", newMessage.Id, newMessage.UserName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendMessage Failed: {ex.GetBaseException().Message}");
            }
        }

        public override Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;

            _logger.LogDebug($"{username} connected to ChatHub");

            userLookup.Add(Context.ConnectionId, username);

            Clients.AllExcept(ConnectionExceptOf(username)).SendAsync("ReceiveMessage", 0, username, $"{username} joined the chat");

            foreach (var message in _dbContext.Messages.ToArray())
                Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", message.Id, message.UserName, message.Text);

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            string id = Context.ConnectionId;

            _logger.LogDebug($"{Context.User.Identity.Name} disconnected from ChatHub");

            if (userLookup.TryGetValue(id, out string username))
            {
                userLookup.Remove(id);
                await Clients.AllExcept(ConnectionExceptOf(username)).SendAsync("ReceiveMessage", 0, username, $"{username} has left the chat");
            }

            await base.OnDisconnectedAsync(e);
        }

        private static List<string> ConnectionExceptOf(string username)
        {
            return userLookup.Where(i => i.Value == username).Select(i => i.Key).ToList();
        }
    }
}
