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
    public class ClientsPage : ComponentBase
    {
        [Inject] IViewNotifier viewNotifier { get; set; }
        [Inject] HttpClient Http { get; set; }
        [Inject] protected IStringLocalizer<Global> L { get; set; }
        int pageSize { get; set; } = 15;
        int currentPage { get; set; } = 0;

        protected bool isCurrentClientKeyReadOnly = false;

        protected List<ClientDto> clients;
        protected ClientDto currentClient { get; set; } = new();
        protected Secret currentSecret { get; set; } = new();
        protected TokenUsage[] TokenUsages { get; set; } = (TokenUsage[])Enum.GetValues(typeof(TokenUsage));
        protected AccessTokenType[] AccessTokenTypes { get; set; } = (AccessTokenType[])Enum.GetValues(typeof(AccessTokenType));
        protected TokenExpiration[] RefreshTokenExpirations { get; set; } = (TokenExpiration[])Enum.GetValues(typeof(TokenExpiration));

        #region OnInitializedAsync

        protected override async Task OnInitializedAsync()
        {
            await InitializeClientsListAsync();
        }

        public async Task InitializeClientsListAsync()
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

        #endregion

        #region OpenUpsertClientDialog
        protected bool isUpsertClientDialogOpen = false;
        bool isInsertOperation;

        protected string labelUpsertDialogTitle;
        protected string labelUpsertDialogOkButton;

        protected List<SelectItem<string>> grantTypeSelections = new();
        protected List<SelectItem<string>> standardScopeSelections = new();
        protected List<SelectItem<string>> apiScopeSelections = new();
        protected List<SelectItem<string>> tokenSigningAlgorithmsSelections = new();

        //See https://identityserver4.readthedocs.io/en/latest/topics/crypto.html
        readonly string[] signingAlgorithms = new string[] { "RS256", "RS384", "RS512", "PS256", "PS384", "PS512", "ES256", "ES384", "ES512" };


        protected async Task OpenUpsertClientDialog(ClientDto client = null)
        {
            try
            {
                var getIdentityResourcesTask = Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<IdentityResource>>>("api/Admin/IdentityResources").ContinueWith((t) =>
                {
                    standardScopeSelections.Clear();

                    foreach (var item in t.Result.Result)
                    {
                        standardScopeSelections.Add(new SelectItem<string>
                        {
                            DisplayValue = $"{item.DisplayName} ({item.Name})",
                            Id = item.Name,
                            Selected = currentClient.AllowedScopes.Contains(item.Name)
                        });
                    }
                });
                var getApiResourcesTask = Http.GetNewtonsoftJsonAsync<ApiResponseDto<List<ApiResource>>>("api/Admin/ApiResources").ContinueWith((t) =>
                {
                    apiScopeSelections.Clear();

                    foreach (var item in t.Result.Result)
                    {
                        apiScopeSelections.Add(new SelectItem<string>
                        {
                            DisplayValue = $"{item.DisplayName} ({item.Name})",
                            Id = item.Name,
                            Selected = currentClient.AllowedScopes.Contains(item.Name)
                        });
                    }
                });

                isInsertOperation = client == null;

                currentClient = client ?? new ClientDto();

                // Update the UI
                if (isInsertOperation)
                {
                    labelUpsertDialogTitle = L["New Client"];
                    labelUpsertDialogOkButton = L["Create"];
                }
                else
                {
                    labelUpsertDialogTitle = L["Edit {0}", currentClient.ClientId];
                    labelUpsertDialogOkButton = L["Update"];
                }

                isCurrentClientKeyReadOnly = !isInsertOperation;

                grantTypeSelections.Clear();

                foreach (var info in typeof(GrantType).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                {
                    grantTypeSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = $"{info.Name.Humanize(LetterCasing.Title)} ({info.GetValue(info)})",
                        Id = info.GetValue(info).ToString(),
                        Selected = currentClient.AllowedGrantTypes.Contains(info.GetValue(info))
                    });
                }

                tokenSigningAlgorithmsSelections.Clear();

                foreach (var sa in signingAlgorithms)
                    tokenSigningAlgorithmsSelections.Add(new SelectItem<string>
                    {
                        DisplayValue = sa,
                        Id = sa,
                        Selected = currentClient.AllowedIdentityTokenSigningAlgorithms.Contains(sa)
                    });

                await Task.WhenAll(getIdentityResourcesTask, getApiResourcesTask);

                if (!isInsertOperation)
                    currentClient.SaveState();

                isUpsertClientDialogOpen = true;
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
            isInsertOperation = false;
            await UpsertClient();
        }

        protected void CancelChanges()
        {
            currentClient.RestoreState();
            isUpsertClientDialogOpen = false;
        }

        protected async Task UpsertClient()
        {
            try
            {
                if (currentClient == null)
                {
                    viewNotifier.Show("Client Creation Error", ViewNotifierType.Error, "New Client not found");
                    return;
                }

                if (isUpsertClientDialogOpen)
                {
                    currentClient.AllowedGrantTypes = grantTypeSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                    currentClient.AllowedIdentityTokenSigningAlgorithms = tokenSigningAlgorithmsSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                    currentClient.AllowedScopes = standardScopeSelections.Where(i => i.Selected).Select(i => i.Id).ToList();
                    ((List<string>)currentClient.AllowedScopes).AddRange(apiScopeSelections.Where(i => i.Selected).Select(i => i.Id).ToList());
                }

                ApiResponseDto apiResponse;

                if (isInsertOperation)
                    apiResponse = await Http.PostJsonAsync<ApiResponseDto>("api/Admin/Client", currentClient);
                else
                    apiResponse = await Http.PutJsonAsync<ApiResponseDto>("api/Admin/Client", currentClient);

                if (apiResponse.IsSuccessStatusCode)
                {
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Success);

                    StateHasChanged();
                }
                else
                    viewNotifier.Show(apiResponse.Message, ViewNotifierType.Error);


                await OnInitializedAsync();

                isUpsertClientDialogOpen = false;
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

        #endregion

        #region OpenDeleteClientDialog

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

        #endregion

        #region OpenDeleteClientSecretDialog

        protected bool isDeleteClientSecretDialogOpen = false;

        protected void OpenDeleteClientSecretDialog(Secret secret)
        {
            currentSecret = secret;
            isDeleteClientSecretDialogOpen = true;
        }

        protected void DeleteClientSecret()
        {
            currentClient?.ClientSecrets.Remove(currentSecret);
            isDeleteClientSecretDialogOpen = false;
        }

        #endregion

        #region OpenCreateClientSecretDialogOpen

        protected bool isCreateClientSecretDialogOpen = false;

        protected void OpenCreateClientSecretDialogOpen()
        {
            currentSecret = new Secret();
            GenerateSecret();
            isCreateClientSecretDialogOpen = true;
        }

        protected void CreateSecret()
        {
            if (!string.IsNullOrWhiteSpace(currentSecret.Value))
            {
                currentSecret.Value = currentSecret.Value.ToSha512();

                if (string.IsNullOrWhiteSpace(currentSecret.Description))
                    currentSecret.Description = $"Created on {DateTime.Now}";

                currentClient?.ClientSecrets.Add(currentSecret);
                isCreateClientSecretDialogOpen = false;
            }
        }

        protected void GenerateSecret()
        {
            currentSecret.Value = CryptoRandom.CreateUniqueId(32);
        }

        #endregion

    }
}
