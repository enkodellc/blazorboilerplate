namespace BlazorBoilerplate.Theme.Material.Shared.Layouts
{
    public partial class MainLayout : IDisposable
    {
        bool _navMenuOpened = true;

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
    }
}
