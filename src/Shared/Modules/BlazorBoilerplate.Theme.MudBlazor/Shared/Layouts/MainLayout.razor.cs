using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Theme.Material.Shared.Layouts
{
    public partial class MainLayout : IDisposable
    {
        [Inject] protected NavigationManager navigationManager { get; set; }
        [Inject] protected IViewNotifier viewNotifier { get; set; }
        [Inject] protected IApiClient apiClient { get; set; }
        [Inject] protected AppState appState { get; set; }
        [Inject] protected HubClient hubClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        bool _navMenuOpened = true;

        [CascadingParameter]
        Task<AuthenticationState> authenticationStateTask { get; set; }
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
            if (e.Notification.NotificationType == NotificationType.OperationCompleted)
                viewNotifier.Show($"{e.Notification.Value}", e.Notification.Success ? ViewNotifierType.Success : ViewNotifierType.Error, "Operation completed");
        }

        protected override async Task OnInitializedAsync()
        {
            var user = (await authenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
            {
                var profile = await appState.GetUserProfile();

                _navMenuOpened = profile.IsNavOpen;
            }
        }

        private void DrawerToggle()
        {
            _navMenuOpened = !_navMenuOpened;
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
