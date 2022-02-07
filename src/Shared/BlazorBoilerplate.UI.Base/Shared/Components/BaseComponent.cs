using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.UI.Base.Shared.Components
{
    public abstract class BaseComponent : ComponentBase, IDisposable
    {
        [CascadingParameter] protected Task<AuthenticationState> authenticationStateTask { get; set; }
        [Inject] protected IAuthorizationService authorizationService { get; set; }
        [Inject] protected NavigationManager navigationManager { get; set; }
        [Inject] protected IViewNotifier viewNotifier { get; set; }
        [Inject] protected IApiClient apiClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        public virtual void Dispose()
        {
            apiClient.CancelChanges();
            apiClient.ClearEntitiesCache();
        }
    }
}
