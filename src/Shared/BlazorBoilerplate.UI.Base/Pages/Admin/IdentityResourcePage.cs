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
    public class IdentityResourcePage : BaseComponent
    {
        [Parameter] public string Id { get; set; }
        [Inject] HttpClient Http { get; set; }

        protected bool found = true;
        protected bool isIdentityContextIdReadOnly = false;
        protected IdentityResourceDto IdentityResource { get; set; } = new();

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
                    var apiResponse = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<IdentityResourceDto>>($"api/admin/identityResource/{Id}");

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success, L["Operation Successful"]);
                        IdentityResource = apiResponse.Result;

                        Init(IdentityResource);

                        found = IdentityResource != null;
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
        protected void Init(IdentityResourceDto identityResource = null)
        {
            try
            {
                isInsertOperation = identityResource == null;

                IdentityResource = identityResource ?? new IdentityResourceDto();

                // Update the UI
                if (isInsertOperation)
                {
                    pageTitle = L["New Identity Resource"];
                    mainButtonLabel = L["Create"];
                }
                else
                {
                    pageTitle = L["Edit {0}", IdentityResource.Name];
                    mainButtonLabel = L["Update"];
                }

                isIdentityContextIdReadOnly = !isInsertOperation;

                jwtClaimSelections.Clear();

                foreach (var info in typeof(JwtClaimTypes).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                {
                    jwtClaimSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = $"{info.Name.Humanize(LetterCasing.Title)} ({info.GetValue(info)})",
                        Id = info.GetValue(info).ToString(),
                        Selected = IdentityResource.UserClaims.Contains(info.GetValue(info))
                    });
                }

                IdentityResource.CustomUserClaims = IdentityResource.GetCustomUserClaims().ToList();

                if (!isInsertOperation)
                    IdentityResource.SaveState();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected void CancelChanges()
        {
            IdentityResource.RestoreState();
        }

        protected async Task UpsertIdentityResource()
        {
            try
            {
                if (IdentityResource == null)
                {
                    viewNotifier.Show("IdentityResource Creation Error", ViewNotifierType.Error, "New Identity Resource not found");
                    return;
                }

                IdentityResource.UserClaims = jwtClaimSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                ((List<string>)IdentityResource.UserClaims).AddRange(IdentityResource.CustomUserClaims);

                ApiResponseDto identityResponse;

                if (isInsertOperation)
                    identityResponse = await Http.PostJsonAsync<ApiResponseDto>("api/admin/identityResource", IdentityResource);
                else
                    identityResponse = await Http.PutJsonAsync<ApiResponseDto>("api/admin/identityResource", IdentityResource);

                if (identityResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(identityResponse.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(identityResponse.Message, ViewNotifierType.Error);


                await OnInitializedAsync();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            finally
            {
                IdentityResource.ClearState();
            }
        }
    }
}
