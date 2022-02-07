using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using Breeze.Sharp;
using Karambolo.Common.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class UsersPage : ComponentBase
    {
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] IApiClient apiClient { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }

        protected IdentityAuthenticationStateProvider identityAuthenticationStateProvider;

        protected int pageSize { get; set; } = 10;
        private int pageIndex { get; set; } = 0;
        protected int totalItemsCount { get; set; } = 0;

        protected bool createUserDialogOpen = false;
        protected bool disableCreateUserButton = false;

        protected bool editDialogOpen = false;
        protected bool disableUpdateUserButton = false;

        protected bool deleteUserDialogOpen = false;

        protected bool changePasswordDialogOpen = false;
        protected bool disableChangePasswordButton = false;

        protected List<ApplicationUser> users { get; set; }
        protected List<SelectItem<Guid>> roleSelections { get; set; } = new();
        protected ApplicationUser currentUser { get; set; } = new ApplicationUser();
        protected RegisterViewModel newUserViewModel { get; set; } = new RegisterViewModel();
        protected ChangePasswordViewModel changePasswordViewModel { get; set; } = new ChangePasswordViewModel();

        protected override async Task OnInitializedAsync()
        {
            identityAuthenticationStateProvider = (IdentityAuthenticationStateProvider)authStateProvider;
            await LoadUsers();
            await LoadRoles();
        }

        protected async Task OnPage(int index, int size)
        {
            pageSize = size;
            pageIndex = index;

            await LoadUsers();
        }

        protected virtual async Task Reload()
        {
            await LoadUsers();
        }
        protected async Task LoadUsers()
        {
            try
            {
                apiClient.ClearEntitiesCache();
                var result = await apiClient.GetUsers(null, pageSize, pageIndex * pageSize);
                users = new List<ApplicationUser>(result);
                totalItemsCount = (int)result.InlineCount.Value;

                viewNotifier.Show(L["One item found", Plural.From("{0} items found", totalItemsCount)], ViewNotifierType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
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

                var apiResponse = await identityAuthenticationStateProvider.AdminUpdateUser(new UserViewModel()
                {
                    UserId = currentUser.Id,
                    UserName = currentUser.UserName,
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
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

                var apiResponse = await identityAuthenticationStateProvider.Create(newUserViewModel);

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

                var apiResponse = await identityAuthenticationStateProvider.AdminChangePassword(changePasswordViewModel);

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
