using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Managers
{
    public interface IExternalAuthManager
    {
        Task<string> ExternalSignIn(HttpContext httpContext);
    }
}