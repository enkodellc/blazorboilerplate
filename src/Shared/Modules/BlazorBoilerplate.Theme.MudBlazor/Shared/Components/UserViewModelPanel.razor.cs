using GoogleMapsComponents;
using GoogleMapsComponents.Maps;
using GoogleMapsComponents.Maps.Places;
using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Account;
using BlazorBoilerplate.Shared.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using BlazorBoilerplate.UI.Base.Shared.Components;

namespace BlazorBoilerplate.Theme.Material.Shared.Components
{
    public partial class UserViewModelBasePanel : BaseComponent
    {
        [Inject] IJSRuntime jSRuntime { get; set; }
        [Inject] AuthenticationStateProvider authStateProvider { get; set; }
        [Inject] protected IAccountApiClient accountApiClient { get; set; }

        protected UpdatePasswordViewModel updatePasswordViewModel { get; set; }
        protected ChangePasswordViewModel changePasswordViewModel { get; set; }
        protected AuthenticatorVerificationCodeViewModel authenticatorVerificationCodeViewModel { get; set; }

        IdentityAuthenticationStateProvider identityAuthenticationStateProvider;

        protected string email;
        protected string title;
        protected string deleteTitle;
        protected bool isBusy;
        [Parameter] public UserViewModel Model { get; set; }
        [Parameter] public string BackLink { get; set; }

        protected Marker marker;
        protected GoogleMap map1;
        protected MapOptions mapOptions;
        protected Autocomplete autocomplete;
        protected ElementReference searchBox;

        protected bool BrowserRemembered
        {
            get { return Model.BrowserRemembered; }
            set
            {
                if (Model.BrowserRemembered != value)
                    ForgetTwoFactorClient().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            viewNotifier.Show(t.Exception.Message, ViewNotifierType.Error, L["Operation Failed"]);
                    });
            }
        }

        protected bool TwoFactorEnabled
        {
            get { return Model.TwoFactorEnabled; }
            set
            {
                if (Model.TwoFactorEnabled != value)
                    EnableDisable2fa().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            viewNotifier.Show(t.Exception.Message, ViewNotifierType.Error, L["Operation Failed"]);
                    });
            }
        }

        protected bool isOperator;
        protected override async Task OnInitializedAsync()
        {
            identityAuthenticationStateProvider = (IdentityAuthenticationStateProvider)authStateProvider;

            Model.UserNameEqualsEmail = Model.UserName == Model.Email;

            deleteTitle = L["Delete this user"];

            if (Model.IsAuthenticated)
            {
                updatePasswordViewModel = new();

                ResetAuthenticatorVerificationCodeViewModel();

                deleteTitle = L["Delete me"];
            }
            else if (Model.UserId != null)
            {
                changePasswordViewModel = new() { UserId = Model.UserId.ToString() };

                var user = (await authenticationStateTask).User;

                if (!Model.IsAuthenticated)
                    isOperator = (await authorizationService.AuthorizeAsync(user, Policies.For(UserFeatures.Operator))).Succeeded;
            }

            if (Model.UserId == null)
                title = L["New User"];
            else
                title = Model.FullName;

            mapOptions = new MapOptions
            {
                Zoom = 18,
                Center = new LatLngLiteral
                {
                    Lat = Model.CompanyLatitude ?? DefaultLocation.Latitude,
                    Lng = Model.CompanyLongitude ?? DefaultLocation.Longitude
                },
                MapTypeId = MapTypeId.Roadmap
            };
        }

        protected async Task Update()
        {
            try
            {
                Model.CompanyVatIn = Model.CompanyVatIn?.ToUpper();

                var position = await marker.GetPosition();

                Model.CompanyLatitude = position.Lat;
                Model.CompanyLongitude = position.Lng;

                var response = Model.IsAuthenticated ? await identityAuthenticationStateProvider.UpdateUser(Model) : await identityAuthenticationStateProvider.UpsertUser(Model);

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);

                    //In caso di creazione nuovo gestore
                    if (Model.UserId == null)
                        navigationManager.NavigateTo(BackLink);
                }
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task OnAfterMapInit()
        {
            marker = await Marker.CreateAsync(map1.JsRuntime, new MarkerOptions
            {
                Position = mapOptions.Center,
                Map = map1.InteropObject,
                Draggable = false,
                Animation = Animation.Drop
            });

            autocomplete = await Autocomplete.CreateAsync(map1.JsRuntime, searchBox, new AutocompleteOptions
            {
                StrictBounds = false
            });

            await autocomplete.SetFields(new[] { "address_components", "geometry", "name" });

            await autocomplete.AddListener("place_changed", async () =>
            {
                var place = await autocomplete.GetPlace();

                if (place?.Geometry == null)
                {
                    // message = "L'indirizzo " + place?.Name + " non è stato trovato!";
                }
                else if (place.Geometry.Location != null)
                {
                    await map1.InteropObject.SetCenter(place.Geometry.Location);
                    await map1.InteropObject.SetZoom(18);

                    await marker.SetPosition(place.Geometry.Location);

                    Model.CompanyAddress = place.AddressComponents.FirstOrDefault(i => i.Types.Contains("route"))?.LongName;

                    Model.CompanyAddress = $"{Model.CompanyAddress} {place.AddressComponents.FirstOrDefault(i => i.Types.Contains("street_number"))?.LongName}";

                    Model.CompanyCity = place.AddressComponents.FirstOrDefault(i => i.Types.Contains("locality"))?.LongName;

                    Model.CompanyProvince = place.AddressComponents.FirstOrDefault(i => i.Types.Contains("administrative_area_level_2"))?.ShortName;

                    Model.CompanyZipCode = place.AddressComponents.FirstOrDefault(i => i.Types.Contains("postal_code"))?.ShortName;

                    Model.CompanyCountryCode = place.AddressComponents.FirstOrDefault(i => i.Types.Contains("country"))?.ShortName;

                    //message = "Risultati trovati per " + place.Name;
                }
                else if (place.Geometry.Viewport != null)
                {
                    await map1.InteropObject.FitBounds(place.Geometry.Viewport, 5);
                    //message = "Risultati trovati per " + place.Name;
                }

                StateHasChanged();
            });
        }

        protected async Task UpdateMyPassword()
        {
            try
            {
                var response = await identityAuthenticationStateProvider.UpdatePassword(updatePasswordViewModel);

                if (response.IsSuccessStatusCode)
                    viewNotifier.Show(L["UpdatePasswordSuccessful"], ViewNotifierType.Success);
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Error, L["UpdatePasswordFailed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["UpdatePasswordFailed"]);
            }
        }

        protected async Task UpdateUserPassword()
        {
            try
            {
                var response = await identityAuthenticationStateProvider.AdminChangePassword(changePasswordViewModel);

                if (response.IsSuccessStatusCode)
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success, response.Message);
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Error);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.GetBaseException().Message, ViewNotifierType.Error, L["ResetPasswordFailed"]);
            }
        }

        #region 2fa
        protected async Task EnableAuthenticator()
        {
            var response = await identityAuthenticationStateProvider.EnableAuthenticator(authenticatorVerificationCodeViewModel);

            if (response.IsSuccessStatusCode)
            {
                viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                Model = response.Result;
                authenticatorVerificationCodeViewModel = null;
                StateHasChanged();
                await Task.Delay(1000);
                await jSRuntime.InvokeVoidAsync("interop.scrollToFragment", "twoFactorEnabledCard");
            }
            else
                viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);

        }

        protected async Task DisableAuthenticator()
        {
            try
            {
                var response = await identityAuthenticationStateProvider.DisableAuthenticator();

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                    Model = response.Result;
                    ResetAuthenticatorVerificationCodeViewModel();
                    StateHasChanged();
                }
                else
                    viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }

        }

        async Task ForgetTwoFactorClient()
        {
            if (Model.BrowserRemembered)
            {
                var response = await identityAuthenticationStateProvider.ForgetTwoFactorClient();

                if (response.IsSuccessStatusCode)
                {
                    viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                    Model = response.Result;
                    StateHasChanged();
                }
                else
                {
                    viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);
                }
            }
        }

        async Task EnableDisable2fa()
        {
            var response = Model.TwoFactorEnabled ? await identityAuthenticationStateProvider.Disable2fa() : await identityAuthenticationStateProvider.Enable2fa();

            if (response.IsSuccessStatusCode)
            {
                viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                Model = response.Result;
                ResetAuthenticatorVerificationCodeViewModel();
                StateHasChanged();
            }
            else
            {
                viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        protected async Task Disable2fa()
        {
            var response = await accountApiClient.Disable2fa(GuidUtil.ToCompressedString(Model.UserId.Value));

            if (response.IsSuccessStatusCode)
            {
                viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);
                Model = response.Result;
                StateHasChanged();
            }
            else
            {
                viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }
        }

        private void ResetAuthenticatorVerificationCodeViewModel()
        {
            if (!Model.TwoFactorEnabled && Model.IsAuthenticated)
                authenticatorVerificationCodeViewModel = new();
        }
        #endregion
        protected async Task Delete()
        {
            try
            {
                if (email != null && Model.Email.ToLower() == email.ToLower())
                {
                    isBusy = true;

                    ApiResponseDto response;

                    if (Model.IsAuthenticated)
                        response = await accountApiClient.DeleteMe();
                    else
                        response = await accountApiClient.DeleteUser(Model.UserId.ToString());

                    if (response.IsSuccessStatusCode)
                    {
                        viewNotifier.Show(L["Operation Successful"], ViewNotifierType.Success);

                        if (Model.IsAuthenticated)
                            await identityAuthenticationStateProvider.Logout();
                        else
                            navigationManager.NavigateTo(BackLink);
                    }
                    else
                    {
                        viewNotifier.Show(response.Message, ViewNotifierType.Error, L["Operation Failed"]);
                    }
                }
                else
                    viewNotifier.Show(L["Operation not performed"], ViewNotifierType.Warning);
            }
            catch (Exception ex)
            {
                viewNotifier.Show(ex.Message, ViewNotifierType.Error, L["Operation Failed"]);
            }

            isBusy = false;
        }
    }
}
