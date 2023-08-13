using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.UI.Base.Shared.Components;

namespace BlazorBoilerplate.Theme.Material.Main.Pages
{
    public class AuthenticationTicketsBasePage : ItemsTableBase<ApplicationUser>
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();

            from = "LoggedUsers";
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                hubClient.OnlineUsersReceived += HubClient_OnlineUsersReceived;
            }
        }

        private async Task RefreshOnlineUsers()
        {
            if (items?.Any() == true)
            {
                var onlineUsers = await hubClient.GetOnlineUsers();

                foreach (var user in items)
                    user.IsOnline = onlineUsers.Contains(user.Id);

            }
        }

        protected override async Task LoadItems()
        {
            await base.LoadItems();

            await RefreshOnlineUsers();
        }

        private async void HubClient_OnlineUsersReceived(object sender, OnlineUsersReceivedEventArgs e)
        {
            await RefreshOnlineUsers();
        }

        public override void Dispose()
        {
            hubClient.OnlineUsersReceived -= HubClient_OnlineUsersReceived;

            base.Dispose();
        }
    }
}
