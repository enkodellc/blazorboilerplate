using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IExternalAuthManager
    {
        Task<string> ExternalSignIn(HttpContext httpContext);
    }
}