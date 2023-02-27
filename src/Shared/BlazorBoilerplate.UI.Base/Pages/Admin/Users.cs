using System.ComponentModel;
using System.Timers;
using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Timer = System.Timers.Timer;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class UsersPage : ItemsTableBase<ApplicationUser>
    {
        [Inject]
        private AuthenticationStateProvider _authStateProvider { get; set; }

        private IdentityAuthenticationStateProvider _identityAuthenticationStateProvider;

        protected bool createUserDialogOpen = false;
        protected bool disableCreateUserButton = false;

        protected bool editDialogOpen = false;
        protected bool disableUpdateUserButton = false;

        protected bool deleteUserDialogOpen = false;

        protected bool changePasswordDialogOpen = false;
        protected bool disableChangePasswordButton = false;

        protected List<SelectItem<Guid>> roleSelections { get; set; } = new();
        protected ApplicationUser currentUser { get; set; } = new ApplicationUser();
        protected RegisterViewModel newUserViewModel { get; set; } = new RegisterViewModel();
        protected ChangePasswordViewModel changePasswordViewModel { get; set; } = new ChangePasswordViewModel();

        protected UserFilter userFilter = new();

        private bool _serverCalled = false;

        private bool _waitingServerCall;

        private Timer _timer = new(1500);

        protected override async Task OnInitializedAsync()
        {
            _timer.Elapsed += TimerElapsed;
            userFilter.PropertyChanged += OnFilterChanged;
            _identityAuthenticationStateProvider = (IdentityAuthenticationStateProvider)_authStateProvider;
            from = "Users";
            queryParameters = userFilter;
            await LoadItems();
            await LoadRoles();
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _serverCalled = false;
            _timer.Stop();
            if (_waitingServerCall)
            {
                _waitingServerCall = false;
                await InvokeAsync(CallServer);
            }
        }

        private async void OnFilterChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_serverCalled)
            {
                await CallServer();
            }
            else
            {
                _waitingServerCall = true;
            }
        }

        private async Task CallServer()
        {
            _serverCalled = true;
            _timer.Start();
            await Reload();
        }

        protected void ResetFilter()
        {
            userFilter = new();
        }

        protected async Task<bool> SearchUsers()
        {
            await Reload();
            return true;
        }

        protected async Task LoadRoles()
        {
            try
            {
                var result = await apiClient.GetRoles();

                roleSelections = result.Select(i => new SelectItem<Guid>
                {
                    Id = i.Id,
                    DisplayValue = i.Name,
                    Selected = false
                }).ToList();

            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected void OpenEditDialog(ApplicationUser user)
        {
            currentUser = user;

            foreach (var role in roleSelections)
                role.Selected = user.UserRoles.Any(i => i.RoleId == role.Id);

            editDialogOpen = true;
        }

        protected void OpenResetPasswordDialog(ApplicationUser user)
        {
            currentUser = user;

            changePasswordViewModel = new ChangePasswordViewModel() { UserId = user.Id.ToString() };

            changePasswordDialogOpen = true;
        }

        protected void OpenDeleteDialog(ApplicationUser user)
        {
            currentUser = user;
            deleteUserDialogOpen = true;
        }

        protected void UpdateUserRole(SelectItem<Guid> roleSelectionItem)
        {
            if (currentUser.UserName.ToLower() != DefaultUserNames.Administrator || roleSelectionItem.DisplayValue != DefaultRoleNames.Administrator)
                roleSelectionItem.Selected = !roleSelectionItem.Selected;
        }

        protected void CancelChanges()
        {
            editDialogOpen = false;
        }

        protected async Task UpdateUserAsync()
        {
            try
            {
                disableUpdateUserButton = true;

                var apiResponse = await _identityAuthenticationStateProvider.AdminUpdateUser(new UserViewModel()
                {
                    UserId = currentUser.Id,
                    UserName = currentUser.UserName,
                    FirstName = currentUser.Person?.FirstName,
                    LastName = currentUser.Person?.LastName,
                    Email = currentUser.Email,
                    Roles = roleSelections.Where(i => i.Selected).Select(i => i.DisplayValue).ToList()
                });

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success);
                    await Reload();
                    editDialogOpen = false;
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            finally
            {
                disableUpdateUserButton = false;
            }
        }

        protected async Task CreateUserAsync()
        {
            try
            {
                disableCreateUserButton = true;

                var apiResponse = await _identityAuthenticationStateProvider.Create(newUserViewModel);

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success);
                    await Reload();
                    newUserViewModel = new RegisterViewModel();
                    createUserDialogOpen = false;
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["UserCreationFailed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["UserCreationFailed"]);
            }
            finally
            {
                disableCreateUserButton = false;
            }
        }

        protected async Task ResetUserPasswordAsync()
        {
            try
            {
                disableChangePasswordButton = true;

                var apiResponse = await _identityAuthenticationStateProvider.AdminChangePassword(changePasswordViewModel);

                if (apiResponse.IsSuccessStatusCode)
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success, apiResponse.Message);
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error);

                changePasswordDialogOpen = false;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["ResetPasswordFailed"]);
            }
            finally
            {
                disableChangePasswordButton = false;
            }
        }

        protected async Task DeleteUserAsync()
        {
            try
            {
                apiClient.RemoveEntity(currentUser);
                await apiClient.SaveChanges();
                viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                deleteUserDialogOpen = false;
                await Reload();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }
    }
}
