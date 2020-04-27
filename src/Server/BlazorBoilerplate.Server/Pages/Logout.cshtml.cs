using BlazorBoilerplate.Infrastructure.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly IAccountManager _accountManager;

        public LogoutModel(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }
        public async Task<IActionResult> OnPostAsync(string returnurl)
        {
            await _accountManager.Logout(User);

            if (returnurl == null)
                returnurl = Shared.Settings.LoginPath;
            
            if (!returnurl.StartsWith("/"))
                returnurl = $"/{returnurl}";

            return LocalRedirect(Url.Content($"~{returnurl}"));
        }
    }
}