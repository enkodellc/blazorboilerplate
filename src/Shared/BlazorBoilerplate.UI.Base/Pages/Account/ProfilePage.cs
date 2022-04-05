using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.UI.Base.Pages.Account
{
    public class ProfilePage : ComponentBase
    {
        [Inject] AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }

        protected UserViewModel userViewModel;
        protected bool updatePasswordDialogOpen = false;
        protected UpdatePasswordViewModel updatePasswordViewModel { get; set; } = new UpdatePasswordViewModel();
        protected AuthenticatorVerificationCodeViewModel authenticatorVerificationCodeViewModel { get; set; } = new AuthenticatorVerificationCodeViewModel();

        IdentityAuthenticationStateProvider identityAuthenticationStateProvider;

        protected bool BrowserRemembered
        {
            get { return userViewModel.BrowserRemembered; }
            set
            {
                if (userViewModel.BrowserRemembered != value)
                    ForgetTwoFactorClient().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            viewNotifier.Show(t.Exception.Message, ViewNotifierType.Error, L["Operation Failed"]);
                    });
            }
        }

        protected bool TwoFactorEnabled
        {
            get { return userViewModel.TwoFactorEnabled; }
            set
            {
                if (userViewModel.TwoFactorEnabled != value)
                    EnableDisable2fa().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            viewNotifier.Show(t.Exception.Message, ViewNotifierType.Error, L["Operation Failed"]);
                    });
            }
        }


        protected override async Task OnInitializedAsync()
        {
            identityAuthenticationStateProvider = (IdentityAuthenticationStateProvider)authStateProvider;

            userViewModel = await identityAuthenticationStateProvider.GetUserViewModel();
        }

        protected void OpenUpdatePasswordDialog()
        {
            updatePasswordViewModel = new UpdatePasswordViewModel();
            updatePasswordDialogOpen = true;
        }

        protected async Task UpdatePassword()
        {
            if (updatePasswordViewModel.NewPassword != updatePasswordViewModel.NewPasswordConfirm)
            {
                viewNotifier.Show(L["PasswordConfirmationFailed"], ViewNotifierType.Warning);
            }
            else
            {
                try
                {
                    var apiResponse = await identityAuthenticationStateProvider.UpdatePassword(updatePasswordViewModel);

                    if (apiResponse.IsSuccessStatusCode)
                        viewNotifier.Show(L["UpdatePasswordSuccessful"], ViewNotifierType.Success);
                    else
                        viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["UpdatePasswordFailed"]);
                }
                catch (Exception ex)
                {
                    viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["UpdatePasswordFailed"]);
                }

                updatePasswordDialogOpen = false;
            }
        }

        protected async Task UpdateUser()
        {
            try
            {
                var apiResponse = await identityAuthenticationStateProvider.UpdateUser(userViewModel);

                if (apiResponse.IsSuccessStatusCode)
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task EnableAuthenticator()
        {
            var apiResponse = await identityAuthenticationStateProvider.EnableAuthenticator(authenticatorVerificationCodeViewModel);

            if (apiResponse.IsSuccessStatusCode)
            {
                viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                userViewModel = apiResponse.Result;
                StateHasChanged();
            }
            else
                viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);

        }

        protected async Task DisableAuthenticator()
        {
            try
            {
                var apiResponse = await identityAuthenticationStateProvider.DisableAuthenticator();

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                    userViewModel = apiResponse.Result;
                    authenticatorVerificationCodeViewModel = new AuthenticatorVerificationCodeViewModel();
                    StateHasChanged();
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }

        }

        async Task ForgetTwoFactorClient()
        {
            if (userViewModel.BrowserRemembered)
            {
                var apiResponse = await identityAuthenticationStateProvider.ForgetTwoFactorClient();

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                    userViewModel = apiResponse.Result;
                    StateHasChanged();
                }
                else
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
                }
            }
        }

        async Task EnableDisable2fa()
        {
            var apiResponse = userViewModel.TwoFactorEnabled ? await identityAuthenticationStateProvider.Disable2fa() : await identityAuthenticationStateProvider.Enable2fa();

            if (apiResponse.IsSuccessStatusCode)
            {
                viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                userViewModel = apiResponse.Result;
                StateHasChanged();
            }
            else
            {
                viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }
    }
}
