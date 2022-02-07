using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Models;
using Humanizer;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Net;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class IdentityResourcesPage : ComponentBase
    {
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] HttpClient Http { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        int pageSize { get; set; } = 15;
        int currentPage { get; set; } = 0;

        protected bool isCurrentIdentityContextIdReadOnly = false;

        protected List<IdentityResourceDto> identityResources;
        protected IdentityResourceDto currentIdentityResource { get; set; } = new();
        protected Secret currentSecret { get; set; } = new();

        #region OnInitializedAsync

        protected override async Task OnInitializedAsync()
        {
            await InitializeIdentityResourcesListAsync();
        }

        protected async Task InitializeIdentityResourcesListAsync()
        {
            try
            {
                var identityResponse = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<IdentityResourceDto>>>($"api/Admin/IdentityResources?pageSize={pageSize}&pageNumber={currentPage}");

                if (identityResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(identityResponse.Message, ViewNotifierType.Success, L["Operation Successful"]);
                    identityResources = identityResponse.Result;
                }
                else
                    viewNotifier.Show(identityResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        #endregion

        #region OpenUpsertIdentityResourceDialog
        protected bool isUpsertIdentityResourceDialogOpen = false;
        bool isInsertOperation;

        protected string labelUpsertDialogTitle;
        protected string labelUpsertDialogOkButton;
        protected List<SelectItem<string>> jwtClaimSelections = new();
        protected void OpenUpsertIdentityResourceDialog(IdentityResourceDto identityResource = null)
        {
            try
            {
                isInsertOperation = identityResource == null;

                currentIdentityResource = identityResource ?? new IdentityResourceDto();

                // Update the UI
                if (isInsertOperation)
                {
                    labelUpsertDialogTitle = L["New Identity Resource"];
                    labelUpsertDialogOkButton = L["Create"];
                }
                else
                {
                    labelUpsertDialogTitle = L["Edit {0}", currentIdentityResource.Name];
                    labelUpsertDialogOkButton = L["Update"];
                }

                isCurrentIdentityContextIdReadOnly = !isInsertOperation;

                jwtClaimSelections.Clear();

                foreach (var info in typeof(JwtClaimTypes).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                {
                    jwtClaimSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = $"{info.Name.Humanize(LetterCasing.Title)} ({info.GetValue(info)})",
                        Id = info.GetValue(info).ToString(),
                        Selected = currentIdentityResource.UserClaims.Contains(info.GetValue(info))
                    });
                }

                currentIdentityResource.CustomUserClaims = currentIdentityResource.GetCustomUserClaims().ToList();

                if (!isInsertOperation)
                    currentIdentityResource.SaveState();

                isUpsertIdentityResourceDialogOpen = true;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task UpdateEnabled(IdentityResourceDto identityResource)
        {
            currentIdentityResource = identityResource;
            currentIdentityResource.Enabled = !currentIdentityResource.Enabled;
            isInsertOperation = false;
            await UpsertIdentityResource();
        }

        protected void CancelChanges()
        {
            currentIdentityResource.RestoreState();
            isUpsertIdentityResourceDialogOpen = false;
        }

        protected async Task UpsertIdentityResource()
        {
            try
            {
                if (currentIdentityResource == null)
                {
                    viewNotifier.Show("IdentityResource Creation Error", ViewNotifierType.Error, "New Identity Resource not found");
                    return;
                }

                if (isUpsertIdentityResourceDialogOpen)
                {
                    currentIdentityResource.UserClaims = jwtClaimSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                    ((List<string>)currentIdentityResource.UserClaims).AddRange(currentIdentityResource.CustomUserClaims);
                }

                ApiResponseDto identityResponse;

                if (isInsertOperation)
                    identityResponse = await Http.PostJsonAsync<ApiResponseDto>("api/Admin/IdentityResource", currentIdentityResource);
                else
                    identityResponse = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/IdentityResource", currentIdentityResource);

                if (identityResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(identityResponse.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(identityResponse.Message, ViewNotifierType.Error);


                await OnInitializedAsync();

                isUpsertIdentityResourceDialogOpen = false;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            finally
            {
                currentIdentityResource.ClearState();
            }
        }

        #endregion

        #region OpenDeleteIdentityResourceDialog

        protected bool isDeleteIdentityResourceDialogOpen = false;

        protected void OpenDeleteIdentityResourceDialog(IdentityResourceDto identityResource)
        {
            currentIdentityResource = identityResource;
            isDeleteIdentityResourceDialogOpen = true;
        }

        protected async Task DeleteIdentityResourceAsync()
        {
            try
            {
                var response = await Http.DeleteAsync($"api/Admin/IdentityResource/{currentIdentityResource.Name}");
                if (response.StatusCode != (HttpStatusCode)Status200OK)
                {
                    viewNotifier.Show("Identity Resource Delete Failed", ViewNotifierType.Error);
                    return;
                }

                viewNotifier.Show("Identity Resource Deleted", ViewNotifierType.Success);
                await OnInitializedAsync();
                isDeleteIdentityResourceDialogOpen = false;
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
