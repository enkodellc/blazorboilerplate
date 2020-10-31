using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using Breeze.Sharp;
using Karambolo.Common.Localization;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Theme.Material.Admin.Pages.Admin
{
    public class UsersPage : ComponentBase
    {
        [Inject] IMatToaster matToaster { get; set; }
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
        protected List<RoleSelection> roleSelections { get; set; } = new List<RoleSelection>();
        protected ApplicationUser currentUser { get; set; } = new ApplicationUser();
        protected RegisterViewModel newUserViewModel { get; set; } = new RegisterViewModel();
        protected ChangePasswordViewModel changePasswordViewModel { get; set; } = new ChangePasswordViewModel();

        protected class RoleSelection
        {
            public Guid RoleId { get; set; }
            public bool IsSelected { get; set; }
            public string Name { get; set; }
        };

        protected override async Task OnInitializedAsync()
        {
            identityAuthenticationStateProvider = (IdentityAuthenticationStateProvider)authStateProvider;
            await LoadUsers();
            await LoadRoles();
        }

        protected async Task OnPage(MatPaginatorPageEvent e)
        {
            pageSize = e.PageSize;
            pageIndex = e.PageIndex;

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

                matToaster.Add(L["One item found", Plural.From("{0} items found", totalItemsCount)], MatToastType.Success, L["Operation Successful"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        protected async Task LoadRoles()
        {
            try
            {
                var result = await apiClient.GetRoles();

                roleSelections = result.Select(i => new RoleSelection
                {
                    RoleId = i.Id,
                    Name = i.Name,
                    IsSelected = false
                }).ToList();

            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        protected void OpenEditDialog(ApplicationUser user)
        {
            currentUser = user;

            foreach (var role in roleSelections)
                role.IsSelected = user.UserRoles.Any(i => i.RoleId == role.RoleId);

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

        protected void UpdateUserRole(RoleSelection roleSelectionItem)
        {
            if (currentUser.UserName.ToLower() != DefaultUserNames.Administrator || roleSelectionItem.Name != DefaultRoleNames.Administrator)
                roleSelectionItem.IsSelected = !roleSelectionItem.IsSelected;
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
                    Roles = roleSelections.Where(i => i.IsSelected).Select(i => i.Name).ToList()
                });

                if (apiResponse.IsSuccessStatusCode)
                {
                    matToaster.Add(apiResponse.Message, MatToastType.Success);
                    await LoadUsers();
                    editDialogOpen = false;
                }
                else
                    matToaster.Add(apiResponse.Message, MatToastType.Danger, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
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
                    matToaster.Add(apiResponse.Message, MatToastType.Success);
                    await LoadUsers();
                    newUserViewModel = new RegisterViewModel();
                    createUserDialogOpen = false;
                }
                else
                    matToaster.Add(apiResponse.Message, MatToastType.Danger, L["UserCreationFailed"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["UserCreationFailed"]);
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
                    matToaster.Add(L["Operation Successful"], MatToastType.Success, apiResponse.Message);
                else
                    matToaster.Add(apiResponse.Message, MatToastType.Danger);

                changePasswordDialogOpen = false;
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["ResetPasswordFailed"]);
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
                matToaster.Add(L["Operation Successful"], MatToastType.Success);
                deleteUserDialogOpen = false;
                await LoadUsers();
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }
    }
}
