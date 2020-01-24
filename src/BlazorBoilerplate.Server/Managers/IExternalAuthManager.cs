using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BlazorBoilerplate.Server.Managers
{
    public interface IExternalAuthManager
    {
        Task<IActionResult> Challenge(string provider);
        Task<IActionResult> ExternalSignIn();
    }
}