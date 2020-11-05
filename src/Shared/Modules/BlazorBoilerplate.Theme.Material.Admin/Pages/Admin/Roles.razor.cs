using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Extensions;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Theme.Material.Admin.Pages.Admin
{
    public class RolesPage : ComponentBase
    {
        [Inject] HttpClient Http { get; set; }
        [Inject] IMatToaster matToaster { get; set; }
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
                    matToaster.Add(apiResponse.Message, MatToastType.Success, L["Operation Successful"]);
                    roles = apiResponse.Result;
                }
                else
                    matToaster.Add(apiResponse.Message, MatToastType.Danger, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        #endregion

        #region OpenUpsertRoleDialog

        protected bool isUpsertRoleDialogOpen = false;
        protected bool isInsertOperation;
        protected List<PermissionSelection> permissionsSelections = new List<PermissionSelection>();

        protected string labelUpsertDialogTitle;
        protected string labelUpsertDialogOkButton;

        public class PermissionSelection
        {
            public bool IsSelected { get; set; }
            public string Name { get; set; }
        };

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
                    permissionsSelections.Add(new PermissionSelection
                    {
                        Name = name,
                        IsSelected = role != null && role.Permissions.Contains(name)
                    });

                isUpsertRoleDialogOpen = true;
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        protected async Task UpsertRole()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentRoleName))
                {
                    matToaster.Add("Role Creation Error", MatToastType.Danger, "Enter in a Role Name");
                    return;
                }

                RoleDto request = new RoleDto
                {
                    Name = currentRoleName,
                    Permissions = permissionsSelections.Where(i => i.IsSelected).Select(i => i.Name).ToList()
                };

                ApiResponseDto apiResponse;

                if (isInsertOperation)
                    apiResponse = await Http.PostJsonAsync<ApiResponseDto>("api/Admin/Role", request);
                else
                    apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/Role", request);

                if (apiResponse.IsSuccessStatusCode)
                {
                    matToaster.Add(apiResponse.Message, MatToastType.Success);

                    StateHasChanged();
                }
                else
                    matToaster.Add(apiResponse.Message, MatToastType.Danger);


                // this.StateHasChanged();
                await OnInitializedAsync();

                isUpsertRoleDialogOpen = false;
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
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
                    matToaster.Add(L["Operation Successful"], MatToastType.Success);
                    await OnInitializedAsync();
                    isDeleteRoleDialogOpen = false;
                    StateHasChanged();                    
                }
                else
                    matToaster.Add(L["Operation Failed"], MatToastType.Danger);
            }
            catch (Exception ex)
            {
                matToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, L["Operation Failed"]);
            }
        }

        #endregion
    }
}
