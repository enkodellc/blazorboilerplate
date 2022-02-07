using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.UI.Base.Pages.IdentityServer
{
    public class IdentityErrorPage : ComponentBase
    {
        [Inject] NavigationManager navigationManager { get; set; }
        [Inject] HttpClient Http { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }

        protected string Description { get; set; }

        protected string errorText;


        protected override async Task OnInitializedAsync()
        {
            if (navigationManager.TryGetQueryString("errorId", out string errorId))
            {
                if (!string.IsNullOrEmpty(errorId))
                {
                    var apiResponse = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<ErrorMessage>>($"api/IdentityServer/GetError?errorId={errorId}");
                    if (apiResponse.IsSuccessStatusCode)
                    {
                        errorText = apiResponse.Result.Error;
                        Description = apiResponse.Result.ErrorDescription;
                    }
                    else
                    {
                        viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error);
                    }
                }
            }
        }
    }
}
