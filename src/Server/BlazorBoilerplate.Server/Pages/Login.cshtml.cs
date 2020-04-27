using BlazorBoilerplate.Server.Managers;
using BlazorBoilerplate.Shared.Dto.Account;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using BlazorBoilerplate.Infrastructure.Server;

namespace BlazorBoilerplate.Server.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAccountManager _accountManager;

        public LoginModel(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }
        public async Task<IActionResult> OnPostAsync(LoginInputModel loginParameters)
        {
            if (ModelState.IsValid)
            {
                var response = await _accountManager.Login(loginParameters);

                if (loginParameters.ReturnUrl == null)
                {
                    if (response.StatusCode == Status200OK)
                        loginParameters.ReturnUrl = response.Message;
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
