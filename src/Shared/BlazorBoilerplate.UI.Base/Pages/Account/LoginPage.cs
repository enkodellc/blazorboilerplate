using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.UI.Base.Pages.Account
{
    public class LoginPage : ComponentBase
    {
        [Inject] NavigationManager navigationManager { get; set; }
        [Inject] AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] protected AppState appState { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }

        [CascadingParameter]
        Task<AuthenticationState> authenticationStateTask { get; set; }

        private string navigateTo = null;
        private IdentityAuthenticationStateProvider identityAuthenticationStateProvider;
        protected bool forgotPasswordToggle = false;
        protected LoginViewModel loginViewModel;
        protected ForgotPasswordViewModel forgotPasswordViewModel { get; set; } = new ForgotPasswordViewModel();

        private string ReturnUrl;

        protected override async Task OnInitializedAsync()
        {
            if (navigationManager.TryGetQueryString("ReturnUrl", out string url))
                ReturnUrl = url;

            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
                navigationManager.NavigateTo(ReturnUrl ?? "/");
            else
            {
                identityAuthenticationStateProvider = (IdentityAuthenticationStateProvider)authStateProvider;

                var apiResponse = await identityAuthenticationStateProvider.BuildLoginViewModel(ReturnUrl);

                if (apiResponse.IsSuccessStatusCode)
                {
                    loginViewModel = apiResponse.Result;

                    if (loginViewModel.IsExternalLoginOnly)
                    {
                        if (!string.IsNullOrEmpty(ReturnUrl))
                            ReturnUrl = Uri.EscapeDataString(ReturnUrl);
                        // we only have one option for logging in and it's an external provider
                        navigationManager.NavigateTo($"{navigationManager.BaseUri}api/externalauth/challenge/{loginViewModel.ExternalLoginScheme}/{ReturnUrl}", true);
                    }
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["LoginFailed"]);
            }
        }

        protected void SignInWith(ExternalProvider provider)
        {
            if (!string.IsNullOrEmpty(ReturnUrl))
                ReturnUrl = Uri.EscapeDataString(ReturnUrl);

            navigationManager.NavigateTo($"{navigationManager.BaseUri}api/externalauth/challenge/{provider.AuthenticationScheme}/{ReturnUrl}", true);
        }

        protected void Register()
        {
            navigationManager.NavigateTo("/account/register");
        }

        protected async Task SubmitLogin()
        {
            try
            {
                loginViewModel.ReturnUrl = ReturnUrl;
                var response = await identityAuthenticationStateProvider.Login(loginViewModel);

                if (response.IsSuccessStatusCode)
                {
                    if (navigationManager.IsWebAssembly())
                    {
                        if (response.Result?.RequiresTwoFactor == true)
                        {
                            var par = string.IsNullOrEmpty(ReturnUrl) ? string.Empty : $"?returnurl={Uri.EscapeDataString(ReturnUrl)}";
                            navigationManager.NavigateTo($"{BlazorBoilerplate.Constants.Settings.LoginWith2faPath}{par}", true);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ReturnUrl))
                            {
                                var userProfile = await appState.GetUserProfile();

                                navigateTo = navigationManager.BaseUri + (!string.IsNullOrEmpty(userProfile?.LastPageVisited) ? userProfile?.LastPageVisited : "/dashboard");
                            }
                            else
                                navigateTo = ReturnUrl;


                            navigationManager.NavigateTo(navigateTo);
                        }
                    }
                }
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Error);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["LoginFailed"]);
            }
        }

        protected async Task ForgotPassword()
        {
            try
            {
                await identityAuthenticationStateProvider.ForgotPassword(forgotPasswordViewModel);
                viewNotifier.Show(L["ForgotPasswordEmailSent"], ViewNotifierType.Success);
                forgotPasswordViewModel.Email = string.Empty;
                forgotPasswordToggle = false;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["ResetPasswordFailed"]);
            }
        }
    }
}
