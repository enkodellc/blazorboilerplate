using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Net;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class MultiTenancyPage : ComponentBase
    {
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] HttpClient Http { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        int pageSize { get; set; } = 15;
        int currentPage { get; set; } = 0;

        protected bool isCurrentTenantKeyReadOnly = false;

        protected List<TenantDto> tenants;
        protected TenantDto currentTenant { get; set; } = new();

        #region OnInitializedAsync

        protected override async Task OnInitializedAsync()
        {
            await InitializeTenantsListAsync();
        }

        protected string GetTenantUri(TenantDto tenant)
        {
            var builder = new UriBuilder(Navigation.BaseUri)
            {
                Host = tenant.Identifier
            };
            return builder.Uri.ToString();
        }

        protected async Task InitializeTenantsListAsync()
        {
            try
            {
                var response = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<TenantDto>>>($"api/Admin/Tenants?pageSize={pageSize}&pageNumber={currentPage}");

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show(response.Message, ViewNotifierType.Success, L["Operation Successful"]);
                    tenants = response.Result;
                }
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        #endregion

        #region OpenUpsertTenantDialog
        protected bool isUpsertTenantDialogOpen = false;
        bool isInsertOperation;

        protected string labelUpsertDialogTitle;
        protected string labelUpsertDialogOkButton;

        protected void OpenUpsertTenantDialog(TenantDto tenant = null)
        {
            try
            {
                isInsertOperation = tenant == null;

                currentTenant = tenant ?? new TenantDto();

                // Update the UI
                if (isInsertOperation)
                {
                    labelUpsertDialogTitle = L["New Tenant"];
                    labelUpsertDialogOkButton = L["Create"];
                }
                else
                {
                    labelUpsertDialogTitle = L["Edit {0}", currentTenant.Name];
                    labelUpsertDialogOkButton = L["Update"];
                }

                isCurrentTenantKeyReadOnly = !isInsertOperation;

                if (!isInsertOperation)
                    currentTenant.SaveState();

                isUpsertTenantDialogOpen = true;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected void CancelChanges()
        {
            currentTenant.RestoreState();
            isUpsertTenantDialogOpen = false;
        }

        protected async Task UpsertTenant()
        {
            try
            {
                if (currentTenant == null)
                {
                    viewNotifier.Show("Tenant Creation Error", ViewNotifierType.Error, "New Tenant not found");
                    return;
                }

                ApiResponseDto response;

                if (isInsertOperation)
                    response = await Http.PostJsonAsync<ApiResponseDto>("api/Admin/Tenant", currentTenant);
                else
                    response = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/Tenant", currentTenant);

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show(response.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Error);


                await OnInitializedAsync();

                isUpsertTenantDialogOpen = false;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            finally
            {
                currentTenant.ClearState();
            }
        }

        #endregion

        #region OpenDeleteTenantDialog

        protected bool isDeleteTenantDialogOpen = false;

        protected void OpenDeleteTenantDialog(TenantDto tenant)
        {
            currentTenant = tenant;
            isDeleteTenantDialogOpen = true;
        }

        protected async Task DeleteTenantAsync()
        {
            try
            {
                var response = await Http.DeleteAsync($"api/Admin/Tenant/{currentTenant.Name}");
                if (response.StatusCode != (HttpStatusCode)Status200OK)
                {
                    viewNotifier.Show("Tenant Delete Failed", ViewNotifierType.Error);
                    return;
                }

                viewNotifier.Show("Tenant Deleted", ViewNotifierType.Success);
                await OnInitializedAsync();
                isDeleteTenantDialogOpen = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        #endregion
    }
}
