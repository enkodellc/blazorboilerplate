using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorBoilerplate.UI.Base.Pages.Account
{
    public class RegisterPage : BaseComponent
    {
        [Inject] AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] protected AppState appState { get; set; }

        protected RegisterViewModel registerViewModel { get; set; } = new RegisterViewModel();

        protected override async Task OnInitializedAsync()
        {
            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
                navigationManager.NavigateTo("/");
        }

        protected async Task RegisterUser()
        {
            try
            {
                var response = await ((IdentityAuthenticationStateProvider)authStateProvider).Register(registerViewModel);

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show("New User Email Verification was sent to: " + registerViewModel.Email, ViewNotifierType.Success, L["UserCreationSuccessful"]);
                    navigationManager.NavigateTo("/");
                }
                else
                {
                    viewNotifier.Show(response.Message, ViewNotifierType.Error, L["UserCreationFailed"]);
                }
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["UserCreationFailed"]);
            }
        }
    }
}
