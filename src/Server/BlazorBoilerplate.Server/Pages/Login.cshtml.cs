using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAccountManager _accountManager;
        private readonly ApplicationDbContext _context;

        public LoginModel(IAccountManager accountManager, ApplicationDbContext context)
        {
            _accountManager = accountManager;
            _context = context;
        }
        public async Task<IActionResult> OnPostAsync(LoginInputModel loginParameters)
        {
            if (ModelState.IsValid)
            {
                var response = await _accountManager.Login(loginParameters);

                if (loginParameters.ReturnUrl == null)
                {
                    if (response.StatusCode == Status200OK)
                        loginParameters.ReturnUrl = (await _context.UserProfiles.SingleOrDefaultAsync(i => i.ApplicationUser.NormalizedUserName == loginParameters.UserName.ToUpper()))?.LastPageVisited ?? string.Empty;
                    else
                        loginParameters.ReturnUrl = Shared.Settings.LoginPath;
                }
                else if (response.StatusCode != Status200OK)
                    loginParameters.ReturnUrl = $"{Shared.Settings.LoginPath}/{loginParameters.ReturnUrl}";
            }
            else
                loginParameters.ReturnUrl = $"{Shared.Settings.LoginPath}/{loginParameters.ReturnUrl ?? string.Empty}";

            if (!loginParameters.ReturnUrl.StartsWith("/"))
                loginParameters.ReturnUrl = $"/{loginParameters.ReturnUrl}";

            return LocalRedirect(Url.Content($"~{loginParameters.ReturnUrl}"));
        }
    }
}
