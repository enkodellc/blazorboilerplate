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
    public class ClientsPage : BaseComponent
    {
        [Inject] HttpClient Http { get; set; }
        int pageSize { get; set; } = 15;
        int currentPage { get; set; } = 0;

        protected List<ClientDto> clients;
        protected ClientDto currentClient { get; set; } = new();


        protected override async Task OnInitializedAsync()
        {
            try
            {
                var apiResponse = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<ClientDto>>>($"api/Admin/Clients?pageSize={pageSize}&pageNumber={currentPage}");

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success, L["Operation Successful"]);
                    clients = apiResponse.Result;
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task UpdateEnabled(ClientDto client)
        {
            currentClient = client;
            currentClient.Enabled = !currentClient.Enabled;

            try
            {
                ApiResponseDto apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/admin/client", currentClient);

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
                currentClient.ClearState();
            }
        }

        protected bool isDeleteClientDialogOpen = false;

        protected void OpenDeleteClientDialog(ClientDto client)
        {
            currentClient = client;
            isDeleteClientDialogOpen = true;
        }

        protected async Task DeleteClientAsync()
        {
            try
            {
                var response = await Http.DeleteAsync($"api/Admin/Client/{currentClient.ClientId}");
                if (response.StatusCode != (HttpStatusCode)Status200OK)
                {
                    viewNotifier.Show("Client Delete Failed", ViewNotifierType.Error);
                    return;
                }

                viewNotifier.Show("Client Deleted", ViewNotifierType.Success);
                await OnInitializedAsync();
                isDeleteClientDialogOpen = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }
    }
}
