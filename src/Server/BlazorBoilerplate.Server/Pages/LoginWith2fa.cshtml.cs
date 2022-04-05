using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorBoilerplate.Server.Pages
{
    [AllowAnonymous]
    public class LoginWith2faPageModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginWith2faPageModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }
        public async Task<IActionResult> OnPostAsync(LoginWith2faModel loginParameters)
        {
            var result = false;

            if (ModelState.IsValid)
            {
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

                if (user != null)
                {
                    await _signInManager.RefreshSignInAsync(user);

                    if (loginParameters.RememberMachine)
                        await _signInManager.RememberTwoFactorClientAsync(user);

                    result = true;
                }
            }

            if (!result)
                loginParameters.ReturnUrl = $"{Settings.LoginWith2faPath}/{loginParameters.ReturnUrl ?? string.Empty}";

            return LocalRedirect(Url.Content($"~{loginParameters.ReturnUrl}"));
        }
    }
}
