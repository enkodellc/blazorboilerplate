using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using BlazorBoilerplate.Shared.Services;

namespace BlazorBoilerplate.UI.Base.Pages.Account
{
    public class LogoutPage : BaseComponent
    {
        [Inject] AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] protected AppState appState { get; set; }

        private IdentityAuthenticationStateProvider identityAuthenticationStateProvider;
        protected LogoutViewModel logoutViewModel;

        private string logoutId;
        protected override async Task OnInitializedAsync()
        {
            navigationManager.TryGetQueryString("logoutId", out logoutId);

            identityAuthenticationStateProvider = (IdentityAuthenticationStateProvider)authStateProvider;

            try
            {
                var apiResponse = await identityAuthenticationStateProvider.BuildLogoutViewModel(logoutId);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var vm = apiResponse.Result;

                    if (vm.ShowLogoutPrompt == false)
                    {
                        await SubmitLogout();
                    }
                    else
                        logoutViewModel = vm;
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["LogoutFailed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["LogoutFailed"]);
            }
        }

        protected async Task SubmitLogout()
        {
            try
            {
                var apiResponse = await identityAuthenticationStateProvider.Logout(logoutViewModel);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var loggedOutViewModel = apiResponse.Result;

                    navigationManager.NavigateTo(loggedOutViewModel.AutomaticRedirectAfterSignOut ?
                        loggedOutViewModel.PostLogoutRedirectUri : "/", true);
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["LogoutFailed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["LogoutFailed"]);
            }
        }
    }
}
