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
    public class RegisterPage : ComponentBase
    {
        [Inject] NavigationManager navigationManager { get; set; }
        [Inject] AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] protected AppState appState { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }

        [CascadingParameter]
        Task<AuthenticationState> authenticationStateTask { get; set; }

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
