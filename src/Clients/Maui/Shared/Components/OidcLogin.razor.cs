using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Providers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplateMaui.Shared.Components
{
    public partial class OidcLogin
    {
        [Inject] protected OidcAuthenticationStateProvider oidcAuthenticationStateProvider { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var response = await oidcAuthenticationStateProvider.Login();

            if (!response.IsSuccessStatusCode)
                viewNotifier.Show(response.Message, ViewNotifierType.Error, L["LoginFailed"]);
        }
    }
}
