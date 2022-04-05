using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Net.Http.Json;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class RolesPage : ComponentBase
    {
        [Inject] HttpClient Http { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        protected int pageSize { get; set; } = 15;
        protected int currentPage { get; set; } = 0;

        protected string currentRoleName = string.Empty;
        protected bool isCurrentRoleReadOnly = false;

        protected List<RoleDto> roles;

        #region OnInitializedAsync

        protected override async Task OnInitializedAsync()
        {
            await InitializeRolesListAsync();
        }

        public async Task InitializeRolesListAsync()
        {
            try
            {
                var apiResponse = await Http.GetFromJsonAsync<ApiResponseDto<List<RoleDto>>>($"api/Admin/Roles?pageSize={pageSize}&pageNumber={currentPage}");

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success, L["Operation Successful"]);
                    roles = apiResponse.Result;
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        #endregion

        #region OpenUpsertRoleDialog

        protected bool isUpsertRoleDialogOpen = false;
        protected bool isInsertOperation;
        protected List<SelectItem<string>> permissionsSelections = new();

        protected string labelUpsertDialogTitle;
        protected string labelUpsertDialogOkButton;

        protected async Task OpenUpsertRoleDialog(string roleName = "")
        {
            try
            {
                currentRoleName = roleName;

                isInsertOperation = string.IsNullOrWhiteSpace(roleName);

                if (isInsertOperation)
                {
                    labelUpsertDialogTitle = L["New Role"];
                    labelUpsertDialogOkButton = L["Create"];
                }
                else
                {
                    labelUpsertDialogTitle = L["Edit {0}", roleName];
                    labelUpsertDialogOkButton = L["Update"];
                }

                RoleDto role = null;
                isCurrentRoleReadOnly = !isInsertOperation;

                if (isCurrentRoleReadOnly)
                {
                    var roleResponse = await Http.GetFromJsonAsync<ApiResponseDto<RoleDto>>($"api/Admin/Role/{roleName}");
                    role = roleResponse.Result;
                }

                var response = await Http.GetFromJsonAsync<ApiResponseDto<List<string>>>("api/Admin/Permissions");
                permissionsSelections.Clear();


                foreach (var name in response.Result)
                    permissionsSelections.Add(new SelectItem<string>
                    {
                        Id = name,
                        DisplayValue = name,
                        Selected = role != null && role.Permissions.Contains(name)
                    });

                isUpsertRoleDialogOpen = true;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task UpsertRole()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentRoleName))
                {
                    viewNotifier.Show("Role Creation Error", ViewNotifierType.Error, "Enter in a Role Name");
                    return;
                }

                RoleDto request = new()
                {
                    Name = currentRoleName,
                    Permissions = permissionsSelections.Where(i => i.Selected).Select(i => i.Id).ToList()
                };

                ApiResponseDto apiResponse;

                if (isInsertOperation)
                    apiResponse = await Http.PostJsonAsync<ApiResponseDto>("api/Admin/Role", request);
                else
                    apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/Role", request);

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error);


                // this.StateHasChanged();
                await OnInitializedAsync();

                isUpsertRoleDialogOpen = false;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        #endregion

        #region OpenDeleteDialog

        protected bool isDeleteRoleDialogOpen = false;

        protected void OpenDeleteDialog(string roleName)
        {
            currentRoleName = roleName;
            isDeleteRoleDialogOpen = true;
        }

        protected async Task DeleteRoleAsync()
        {
            try
            {
                var response = await Http.DeleteAsync($"api/Admin/Role/{currentRoleName}");

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                    await OnInitializedAsync();
                    isDeleteRoleDialogOpen = false;
                    StateHasChanged();
                }
                else
                    viewNotifier.Show(L["Operation Failed"], ViewNotifierType.Error);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        #endregion
    }
}
