using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using IdentityModel.OidcClient;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplateMaui.Shared.Components
{
    public partial class OidcLogin
    {
        [Inject] protected OidcClient client { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var result = await client.LoginAsync();

                if (!result.IsError)
                {

                }
                else
                    viewNotifier.Show(result.Error, ViewNotifierType.Error, L["LoginFailed"]);

                //Navigation.NavigateTo($"{BlazorBoilerplate.Constants.Settings.LoginPath}?returnurl={Uri.EscapeDataString(Navigation.Uri)}", true);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["LoginFailed"]);
            }
        }
    }
}
