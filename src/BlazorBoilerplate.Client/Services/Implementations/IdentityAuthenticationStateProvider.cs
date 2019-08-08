using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Shared;

namespace BlazorBoilerplate.Client.States
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private UserInfo _userInfoCache = null;
        private readonly IAuthorizeApi _authorizeApi;

        public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi)
        {
            this._authorizeApi = authorizeApi;
        }

        public async Task<ClientApiResponse> Login(LoginParameters loginParameters)
        {
            ClientApiResponse apiResponse = await _authorizeApi.Login(loginParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ClientApiResponse> Register(RegisterParameters registerParameters)
        {
            ClientApiResponse apiResponse = await _authorizeApi.Register(registerParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ClientApiResponse> Logout()
        {
            ClientApiResponse apiResponse = await _authorizeApi.Logout();
            _userInfoCache = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ClientApiResponse> ConfirmEmail(ConfirmEmailParameters confirmEmailParameters)
        {
            ClientApiResponse apiResponse = await _authorizeApi.ConfirmEmail(confirmEmailParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ClientApiResponse> ResetPassword(ResetPasswordParameters resetPasswordParameters)
        {
            ClientApiResponse apiResponse = await _authorizeApi.ResetPassword(resetPasswordParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ClientApiResponse> ForgotPassword(ForgotPasswordParameters forgotPasswordParameters)
        {
            ClientApiResponse apiResponse = await _authorizeApi.ForgotPassword(forgotPasswordParameters);
            return apiResponse;
        }

        public async Task<UserInfo> GetUserInfo()
        {
            if (_userInfoCache != null && _userInfoCache.IsAuthenticated)
            {
                return _userInfoCache;
            }

            _userInfoCache = await _authorizeApi.GetUserInfo();
            return _userInfoCache;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await GetUserInfo();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, _userInfoCache.UserName) }.Concat(_userInfoCache.ExposedClaims.Select(c => new Claim(c.Key, c.Value)));
                    identity = new ClaimsIdentity(claims, "Server authentication");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex.ToString());
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<ClientApiResponse> UpdateUser(UserInfo userInfo)
        {
            ClientApiResponse apiResponse = await _authorizeApi.UpdateUser(userInfo);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }
    }
}
