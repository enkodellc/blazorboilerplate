using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Admin;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.UI.Base.Shared.Components;
using Humanizer;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Components;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.UI.Base.Pages.Admin
{
    public class ApiResourcePage : BaseComponent
    {
        [Parameter] public string Id { get; set; }
        [Inject] HttpClient Http { get; set; }

        protected bool found = true;
        protected bool isApiContextIdReadOnly = false;
        protected ApiResourceDto ApiResource { get; set; } = new();
        protected Secret Secret { get; set; } = new Secret();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (navigationManager.Uri.ToLower().EndsWith("/add"))
                {
                    Init();
                }
                else
                {
                    var apiResponse = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<ApiResourceDto>>($"api/admin/apiResource/{Id}");

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success, L["Operation Successful"]);
                        ApiResource = apiResponse.Result;

                        Init(ApiResource);

                        found = ApiResource != null;
                    }
                    else if (apiResponse.StatusCode != Status404NotFound)
                        viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error, L["Operation Failed"]);
                    else
                        found = false;
                }
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        bool isInsertOperation;

        protected string pageTitle;
        protected string mainButtonLabel;
        protected List<SelectItem<string>> jwtClaimSelections = new();
        protected List<SelectItem<string>> tokenSigningAlgorithmsSelections = new();

        //See https://identityserver4.readthedocs.io/en/latest/topics/crypto.html
        readonly string[] signingAlgorithms = new string[] { "RS256", "RS384", "RS512", "PS256", "PS384", "PS512", "ES256", "ES384", "ES512" };

        protected void Init(ApiResourceDto apiResource = null)
        {
            try
            {
                isInsertOperation = apiResource == null;

                ApiResource = apiResource ?? new ApiResourceDto();

                // Update the UI
                if (isInsertOperation)
                {
                    pageTitle = L["New API Resource"];
                    mainButtonLabel = L["Create"];
                }
                else
                {
                    pageTitle = L["Edit {0}", ApiResource.Name];
                    mainButtonLabel = L["Update"];
                }

                isApiContextIdReadOnly = !isInsertOperation;

                jwtClaimSelections.Clear();

                foreach (var info in typeof(JwtClaimTypes).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                {
                    jwtClaimSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = $"{info.Name.Humanize(LetterCasing.Title)} ({info.GetValue(info)})",
                        Id = info.GetValue(info).ToString(),
                        Selected = ApiResource.UserClaims.Contains(info.GetValue(info))
                    });
                }

                tokenSigningAlgorithmsSelections.Clear();

                foreach (var sa in signingAlgorithms)
                    tokenSigningAlgorithmsSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = sa,
                        Id = sa,
                        Selected = ApiResource.AllowedAccessTokenSigningAlgorithms.Contains(sa)
                    });

                ApiResource.CustomUserClaims = ApiResource.GetCustomUserClaims().ToList();

                if (!isInsertOperation)
                    ApiResource.SaveState();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected void CancelChanges()
        {
            ApiResource.RestoreState();
        }

        protected async Task UpsertApiResource()
        {
            try
            {
                if (ApiResource == null)
                {
                    viewNotifier.Show("ApiResource Creation Error", ViewNotifierType.Error, "New ApiResource not found");
                    return;
                }

                ApiResource.UserClaims = jwtClaimSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                ApiResource.AllowedAccessTokenSigningAlgorithms = tokenSigningAlgorithmsSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                ((List<string>)ApiResource.UserClaims).AddRange(ApiResource.CustomUserClaims);

                ApiResponseDto apiResponse;

                if (isInsertOperation)
                    apiResponse = await Http.PostJsonAsync<ApiResponseDto>("api/admin/apiResource", ApiResource);
                else
                    apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/admin/apiResource", ApiResource);

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error);


                await OnInitializedAsync();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            finally
            {
                ApiResource.ClearState();
            }
        }

        #region OpenDeleteApiResourceSecretDialog

        protected bool isDeleteApiResourceSecretDialogOpen = false;

        protected void OpenDeleteApiResourceSecretDialog(Secret secret)
        {
            Secret = secret;
            isDeleteApiResourceSecretDialogOpen = true;
        }

        protected void DeleteApiResourceSecret()
        {
            ApiResource?.ApiSecrets.Remove(Secret);
            isDeleteApiResourceSecretDialogOpen = false;
        }

        #endregion

        #region OpenCreateApiResourceSecretDialogOpen

        protected bool isCreateApiResourceSecretDialogOpen = false;

        protected void OpenCreateApiResourceSecretDialogOpen()
        {
            Secret = new Secret();
            GenerateSecret();
            isCreateApiResourceSecretDialogOpen = true;
        }

        protected void CreateSecret()
        {
            if (!string.IsNullOrWhiteSpace(Secret.Value))
            {
                Secret.Value = Secret.Value.ToSha512();

                if (string.IsNullOrWhiteSpace(Secret.Description))
                    Secret.Description = $"Created on {DateTime.Now}";

                ApiResource?.ApiSecrets.Add(Secret);
                isCreateApiResourceSecretDialogOpen = false;
            }
        }

        protected void GenerateSecret()
        {
            Secret.Value = CryptoRandom.CreateUniqueId(32);
        }

        #endregion

    }
}
