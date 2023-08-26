using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.UI.Base.Shared.Layouts
{
    public partial class RootLayout : IDisposable
    {
        [Inject] protected AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] protected NavigationManager navigationManager { get; set; }
        [Inject] protected IApiClient apiClient { get; set; }
        [Inject] protected IAccountApiClient accountApiClient { get; set; }
        [Inject] protected AppState appState { get; set; }
        [Inject] protected IViewNotifier viewNotifier { get; set; }
        [Inject] protected HubClient hubClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        [Inject] protected Microsoft.JSInterop.IJSRuntime js { get; set; }

        public RenderFragment TopSection => topSection?.ChildContent;

        TopSection topSection;

        [CascadingParameter]
        protected Task<AuthenticationState> authenticationStateTask { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated)
                {
                    await InitNotificationClient();
                }
            }
        }

        async Task InitNotificationClient()
        {
            try
            {
                if (await hubClient.Start())
                    hubClient.NotificationReceived += HubClient_NotificationReceived;
            }
            catch (Exception e)
            {
                viewNotifier.Show($"Failed to start HubClient: {e.Message}", ViewNotifierType.Error);
            }
        }

        private void HubClient_NotificationReceived(object sender, NotificationReceivedEventArgs e)
        {
            switch (e.Notification.NotificationType)
            {
                case NotificationType.OperationCompleted:
                    viewNotifier.Show($"{e.Notification.Value}", e.Notification.Success ? ViewNotifierType.Success : ViewNotifierType.Error, L["Operation Successful"]);
                    break;

                case NotificationType.ForceLogout:
                    _ = ((IdentityAuthenticationStateProvider)authStateProvider).Logout(new LogoutViewModel { ReturnUrl = navigationManager.Uri });
                    break;
            }
        }

        public void SetTopSection(TopSection topSection)
        {
            this.topSection = topSection;
            Update();
        }

        public void Update()
        {
            if (topSection != null)
                StateHasChanged();
        }

        async Task Disconnect()
        {
            if (hubClient != null)
            {
                await hubClient.Stop();
                hubClient.Dispose();
            }
        }

        public void Dispose()
        {
            _ = Disconnect();

            hubClient.NotificationReceived -= HubClient_NotificationReceived;
        }
    }
}
