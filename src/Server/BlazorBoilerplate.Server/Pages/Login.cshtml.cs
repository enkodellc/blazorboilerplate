using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
            var result = false;

            if (ModelState.IsValid)
            {
                var response = await _accountManager.Login(loginParameters);

                if (response.StatusCode == Status200OK)
                {
                    result = true;

                    if (string.IsNullOrEmpty(loginParameters.ReturnUrl) || loginParameters.ReturnUrl == "/")
                        loginParameters.ReturnUrl = (await _context.UserProfiles.SingleOrDefaultAsync(i => i.ApplicationUser.NormalizedUserName == loginParameters.UserName.ToUpper()))?.LastPageVisited ?? "/";

                    if ((response.Result as LoginResponseModel)?.RequiresTwoFactor == true)
                        loginParameters.ReturnUrl = $"{Settings.LoginWith2faPath}?returnurl={Uri.EscapeDataString(loginParameters.ReturnUrl)}";
                }
            }

            if (!result)
                loginParameters.ReturnUrl = $"{Settings.LoginPath}/{loginParameters.ReturnUrl ?? string.Empty}";

            return LocalRedirect(Url.Content($"~{loginParameters.ReturnUrl}"));
        }
    }
}
