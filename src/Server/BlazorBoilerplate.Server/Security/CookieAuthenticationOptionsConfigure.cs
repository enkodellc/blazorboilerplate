using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlazorBoilerplate.Server.Security
{
    public class CookieAuthenticationOptionsConfigure : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieAuthenticationOptionsConfigure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name == IdentityConstants.ApplicationScheme)
            {
                options.SessionStore = new CacheTicketStore(_httpContextAccessor);
            }
        }

        public void Configure(CookieAuthenticationOptions options)
        {
        }
    }
}
