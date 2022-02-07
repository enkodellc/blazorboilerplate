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
    public class ApiResourcesPage : ComponentBase
    {
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] HttpClient Http { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        int pageSize { get; set; } = 15;
        int currentPage { get; set; } = 0;

        protected bool isCurrentApiContextIdReadOnly = false;

        protected List<ApiResourceDto> apiResources;
        protected ApiResourceDto currentApiResource { get; set; } = new();
        protected Secret currentSecret { get; set; } = new Secret();

        #region OnInitializedAsync

        protected override async Task OnInitializedAsync()
        {
            await InitializeApiResourcesListAsync();
        }

        public async Task InitializeApiResourcesListAsync()
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

        #endregion

        #region OpenUpsertApiResourceDialog

        protected bool isUpsertApiResourceDialogOpen = false;
        bool isInsertOperation;

        protected string labelUpsertDialogTitle;
        protected string labelUpsertDialogOkButton;
        protected List<SelectItem<string>> jwtClaimSelections = new();
        protected List<SelectItem<string>> tokenSigningAlgorithmsSelections = new();

        //See https://identityserver4.readthedocs.io/en/latest/topics/crypto.html
        readonly string[] signingAlgorithms = new string[] { "RS256", "RS384", "RS512", "PS256", "PS384", "PS512", "ES256", "ES384", "ES512" };


        protected void OpenUpsertApiResourceDialog(ApiResourceDto apiResource = null)
        {
            try
            {
                isInsertOperation = apiResource == null;

                currentApiResource = apiResource ?? new ApiResourceDto();

                // Update the UI
                if (isInsertOperation)
                {
                    labelUpsertDialogTitle = L["New API Resource"];
                    labelUpsertDialogOkButton = L["Create"];
                }
                else
                {
                    labelUpsertDialogTitle = L["Edit {0}", currentApiResource.Name];
                    labelUpsertDialogOkButton = L["Update"];
                }

                isCurrentApiContextIdReadOnly = !isInsertOperation;

                jwtClaimSelections.Clear();

                foreach (var info in typeof(JwtClaimTypes).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                {
                    jwtClaimSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = $"{info.Name.Humanize(LetterCasing.Title)} ({info.GetValue(info)})",
                        Id = info.GetValue(info).ToString(),
                        Selected = currentApiResource.UserClaims.Contains(info.GetValue(info))
                    });
                }

                tokenSigningAlgorithmsSelections.Clear();

                foreach (var sa in signingAlgorithms)
                    tokenSigningAlgorithmsSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = sa,
                        Id = sa,
                        Selected = currentApiResource.AllowedAccessTokenSigningAlgorithms.Contains(sa)
                    });

                currentApiResource.CustomUserClaims = currentApiResource.GetCustomUserClaims().ToList();

                if (!isInsertOperation)
                    currentApiResource.SaveState();

                isUpsertApiResourceDialogOpen = true;
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task UpdateEnabled(ApiResourceDto apiResource)
        {
            currentApiResource = apiResource;
            currentApiResource.Enabled = !currentApiResource.Enabled;
            isInsertOperation = false;
            await UpsertApiResource();
        }

        protected void CancelChanges()
        {
            currentApiResource.RestoreState();
            isUpsertApiResourceDialogOpen = false;
        }

        protected async Task UpsertApiResource()
        {
            try
            {
                if (currentApiResource == null)
                {
                    viewNotifier.Show("ApiResource Creation Error", ViewNotifierType.Error, "New ApiResource not found");
                    return;
                }

                if (isUpsertApiResourceDialogOpen)
                {
                    currentApiResource.UserClaims = jwtClaimSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                    currentApiResource.AllowedAccessTokenSigningAlgorithms = tokenSigningAlgorithmsSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                    ((List<string>)currentApiResource.UserClaims).AddRange(currentApiResource.CustomUserClaims);
                }

                ApiResponseDto apiResponse;

                if (isInsertOperation)
                    apiResponse = await Http.PostJsonAsync<ApiResponseDto>("api/Admin/ApiResource", currentApiResource);
                else
                    apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/ApiResource", currentApiResource);

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error);


                await OnInitializedAsync();

                isUpsertApiResourceDialogOpen = false;
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

        #endregion

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
                var response = await Http.DeleteAsync($"api/Admin/ApiResource/{currentApiResource.Name}");
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

        #region OpenDeleteApiResourceSecretDialog

        protected bool isDeleteApiResourceSecretDialogOpen = false;

        protected void OpenDeleteApiResourceSecretDialog(Secret secret)
        {
            currentSecret = secret;
            isDeleteApiResourceSecretDialogOpen = true;
        }

        protected void DeleteApiResourceSecret()
        {
            currentApiResource?.ApiSecrets.Remove(currentSecret);
            isDeleteApiResourceSecretDialogOpen = false;
        }

        #endregion

        #region OpenCreateApiResourceSecretDialogOpen

        protected bool isCreateApiResourceSecretDialogOpen = false;

        protected void OpenCreateApiResourceSecretDialogOpen()
        {
            currentSecret = new Secret();
            GenerateSecret();
            isCreateApiResourceSecretDialogOpen = true;
        }

        protected void CreateSecret()
        {
            if (!string.IsNullOrWhiteSpace(currentSecret.Value))
            {
                currentSecret.Value = currentSecret.Value.ToSha512();

                if (string.IsNullOrWhiteSpace(currentSecret.Description))
                    currentSecret.Description = $"Created on {DateTime.Now}";

                currentApiResource?.ApiSecrets.Add(currentSecret);
                isCreateApiResourceSecretDialogOpen = false;
            }
        }

        protected void GenerateSecret()
        {
            currentSecret.Value = CryptoRandom.CreateUniqueId(32);
        }

        #endregion

    }
}
