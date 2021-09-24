using IdentityModel;
using Microsoft.AspNetCore.SignalR;

namespace BlazorBoilerplate.Server.Providers
{
    public class UserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(JwtClaimTypes.Name)?.Value;
        }
    }
}
