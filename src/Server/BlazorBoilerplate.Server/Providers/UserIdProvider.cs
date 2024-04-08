using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BlazorBoilerplate.Server.Providers
{
    public class UserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
