using Microsoft.AspNetCore.Http;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IExternalAuthManager
    {
        Task<string> ExternalSignIn(HttpContext httpContext);
    }
}