using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Microsoft.AspNetCore.Components;
using System.Net;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class ApiResourcesPage : BaseComponent
    {
        [Inject] HttpClient Http { get; set; }
        int pageSize { get; set; } = 15;
        int currentPage { get; set; } = 0;

        protected bool isCurrentApiContextIdReadOnly = false;

        protected List<ApiResourceDto> apiResources;
        protected ApiResourceDto currentApiResource { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var apiResponse = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<ApiResourceDto>>>($"api/Admin/ApiResources?pageSize={pageSize}&pageNumber={currentPage}");

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success, L["Operation Successful"]);
                    apiResources = apiResponse.Result;
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }


        protected bool isUpsertApiResourceDialogOpen = false;


        protected async Task UpdateEnabled(ApiResourceDto apiResource)
        {
            currentApiResource = apiResource;
            currentApiResource.Enabled = !currentApiResource.Enabled;

            try
            {
                ApiResponseDto apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/ApiResource", currentApiResource);

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            finally
            {
                currentApiResource.ClearState();
            }
        }

        #region OpenDeleteApiResourceDialog

        protected bool isDeleteApiResourceDialogOpen = false;

        protected void OpenDeleteApiResourceDialog(ApiResourceDto apiResource)
        {
            currentApiResource = apiResource;
            isDeleteApiResourceDialogOpen = true;
        }

        protected async Task DeleteApiResourceAsync()
        {
            try
            {
                var response = await Http.DeleteAsync($"api/admin/apiResource/{currentApiResource.Name}");
                if (response.StatusCode != (HttpStatusCode)Status200OK)
                {
                    viewNotifier.Show("ApiResource Delete Failed", ViewNotifierType.Error);
                    return;
                }

                viewNotifier.Show("API Resource Deleted", ViewNotifierType.Success);
                await OnInitializedAsync();
                isDeleteApiResourceDialogOpen = false;
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
