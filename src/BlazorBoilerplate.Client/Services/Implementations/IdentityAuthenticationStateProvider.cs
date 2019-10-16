using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorBoilerplate.Client.States
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private UserInfoDto _userInfoCache = null;
        private readonly IAuthorizeApi _authorizeApi;
        private readonly AppState _appState;
        private readonly NavigationManager _navigationManager;

        public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi, AppState appState, NavigationManager navigationManager)
        {
            _authorizeApi = authorizeApi;
            _appState = appState;
            _navigationManager = navigationManager;
        }

        public async Task<ApiResponseDto> Login(LoginDto loginParameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.Login(loginParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> Register(RegisterDto registerParameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.Register(registerParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> Create(RegisterDto registerParameters)
        {            
            ApiResponseDto apiResponse = await _authorizeApi.Create(registerParameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto> Logout()
        {
            _appState.UserProfile = null;
            ApiResponseDto apiResponse = await _authorizeApi.Logout();
            _userInfoCache = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailDto confirmEmailParameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ConfirmEmail(confirmEmailParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordDto resetPasswordParameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ResetPassword(resetPasswordParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordParameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ForgotPassword(forgotPasswordParameters);
            return apiResponse;
        }

        public async Task<UserInfoDto> GetUserInfo()
        {            
            if (_userInfoCache != null && _userInfoCache.IsAuthenticated)
            {
                return _userInfoCache;
            }

            //If the user is not authenticated then an empt UserInfoDto is returned
            _userInfoCache = await _authorizeApi.GetUserInfo();
            return _userInfoCache;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // hack: create a new HttpClient rather than relying on the registered service as the AuthenticationStateProvider is initialized prior to IUriHelper ( https://github.com/aspnet/AspNetCore/issues/11867 )
            HttpClient http = new HttpClient();
            Uri uri = new Uri(_navigationManager.BaseUri);
            string apiurl = uri.Scheme + "://" + uri.Authority + "/~/api/User/authenticate";
            UserInfoDto user = await http.GetJsonAsync<UserInfoDto>(apiurl);
            var identity = user.IsAuthenticated
                ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName) }, "serverauth")
                : new ClaimsIdentity();
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfoDto userInfo)
        {
            ApiResponseDto apiResponse = await _authorizeApi.UpdateUser(userInfo);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }
    }
}
