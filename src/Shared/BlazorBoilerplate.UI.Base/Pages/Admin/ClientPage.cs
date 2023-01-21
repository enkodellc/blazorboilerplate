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
    public class ClientPage : BaseComponent
    {
        [Parameter] public string Id { get; set; }
        [Inject] HttpClient Http { get; set; }

        protected bool found = true;
        protected bool isClientKeyReadOnly;
        protected ClientDto Client { get; set; } = new();
        protected Secret Secret { get; set; } = new();
        protected TokenUsage[] TokenUsages { get; set; } = (TokenUsage[])Enum.GetValues(typeof(TokenUsage));
        protected AccessTokenType[] AccessTokenTypes { get; set; } = (AccessTokenType[])Enum.GetValues(typeof(AccessTokenType));
        protected TokenExpiration[] RefreshTokenExpirations { get; set; } = (TokenExpiration[])Enum.GetValues(typeof(TokenExpiration));

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (navigationManager.Uri.ToLower().EndsWith("/add"))
                {
                    await Init();
                }
                else
                {
                    var apiResponse = await Http.GetNewtonsoftJsonAsync<ApiResponseDto<ClientDto>>($"api/Admin/Client/{Id}");

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success, L["Operation Successful"]);
                        Client = apiResponse.Result;

                        await Init(Client);

                        found = Client != null;
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

        protected List<SelectItem<string>> grantTypeSelections = new();
        protected List<SelectItem<string>> standardScopeSelections = new();
        protected List<SelectItem<string>> apiScopeSelections = new();
        protected List<SelectItem<string>> tokenSigningAlgorithmsSelections = new();

        //See https://identityserver4.readthedocs.io/en/latest/topics/crypto.html
        readonly string[] signingAlgorithms = new string[] { "RS256", "RS384", "RS512", "PS256", "PS384", "PS512", "ES256", "ES384", "ES512" };

        protected async Task Init(ClientDto client = null)
        {
            try
            {
                var getIdentityResourcesTask = Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<IdentityResource>>>("api/admin/identityResources").ContinueWith((t) =>
                {
                    standardScopeSelections.Clear();

                    foreach (var item in t.Result.Result)
                    {
                        standardScopeSelections.Add(new SelectItem<string>
                        {
                            DisplayValue = $"{item.DisplayName} ({item.Name})",
                            Id = item.Name,
                            Selected = Client.AllowedScopes.Contains(item.Name)
                        });
                    }
                });

                var getApiResourcesTask = Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<ApiResource>>>("api/admin/apiResources").ContinueWith((t) =>
                {
                    apiScopeSelections.Clear();

                    foreach (var item in t.Result.Result)
                    {
                        apiScopeSelections.Add(new SelectItem<string>
                        {
                            DisplayValue = $"{item.DisplayName} ({item.Name})",
                            Id = item.Name,
                            Selected = Client.AllowedScopes.Contains(item.Name)
                        });
                    }
                });

                isInsertOperation = client == null;

                Client = client ?? new ClientDto();

                if (isInsertOperation)
                {
                    pageTitle = L["New Client"];
                    mainButtonLabel = L["Create"];
                }
                else
                {
                    pageTitle = L["Edit {0}", Client.ClientId];
                    mainButtonLabel = L["Update"];
                }

                isClientKeyReadOnly = !isInsertOperation;

                grantTypeSelections.Clear();

                foreach (var info in typeof(GrantType).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                {
                    grantTypeSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = $"{info.Name.Humanize(LetterCasing.Title)} ({info.GetValue(info)})",
                        Id = info.GetValue(info).ToString(),
                        Selected = Client.AllowedGrantTypes.Contains(info.GetValue(info))
                    });
                }

                tokenSigningAlgorithmsSelections.Clear();

                foreach (var sa in signingAlgorithms)
                    tokenSigningAlgorithmsSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = sa,
                        Id = sa,
                        Selected = Client.AllowedIdentityTokenSigningAlgorithms.Contains(sa)
                    });

                await Task.WhenAll(getIdentityResourcesTask, getApiResourcesTask);

                if (!isInsertOperation)
                    Client.SaveState();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected void CancelChanges()
        {
            Client.RestoreState();
        }

        protected async Task UpsertClient()
        {
            try
            {
                if (Client == null)
                {
                    viewNotifier.Show("Client Creation Error", ViewNotifierType.Error, "New Client not found");
                    return;
                }

                Client.AllowedGrantTypes = grantTypeSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                Client.AllowedIdentityTokenSigningAlgorithms = tokenSigningAlgorithmsSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                Client.AllowedScopes = standardScopeSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                ((List<string>)Client.AllowedScopes).AddRange(apiScopeSelections.Where(i => i.Selected).Select(i => i.Id).ToList());

                ApiResponseDto apiResponse;

                if (isInsertOperation)
                    apiResponse = await Http.PostJsonAsync<ApiResponseDto>("api/Admin/Client", Client);
                else
                    apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/Client", Client);

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
                Client.ClearState();
            }
        }

        #region OpenDeleteClientSecretDialog

        protected bool isDeleteClientSecretDialogOpen = false;

        protected void OpenDeleteClientSecretDialog(Secret secret)
        {
            this.Secret = secret;
            isDeleteClientSecretDialogOpen = true;
        }

        protected void DeleteClientSecret()
        {
            Client?.ClientSecrets.Remove(Secret);
            isDeleteClientSecretDialogOpen = false;
        }

        #endregion

        #region OpenCreateClientSecretDialogOpen

        protected bool isCreateClientSecretDialogOpen = false;

        protected void OpenCreateClientSecretDialogOpen()
        {
            Secret = new Secret();
            GenerateSecret();
            isCreateClientSecretDialogOpen = true;
        }

        protected void CreateSecret()
        {
            if (!string.IsNullOrWhiteSpace(Secret.Value))
            {
                Secret.Value = Secret.Value.ToSha512();

                if (string.IsNullOrWhiteSpace(Secret.Description))
                    Secret.Description = $"Created on {DateTime.Now}";

                Client?.ClientSecrets.Add(Secret);
                isCreateClientSecretDialogOpen = false;
            }
        }

        protected void GenerateSecret()
        {
            Secret.Value = CryptoRandom.CreateUniqueId(32);
        }

        #endregion

    }
}
