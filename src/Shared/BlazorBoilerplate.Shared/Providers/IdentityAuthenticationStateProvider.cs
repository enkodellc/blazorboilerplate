using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Dto.Account;

namespace BlazorBoilerplate.Shared.Providers
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthorizeApi _authorizeApi;

        public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi)
        {
            _authorizeApi = authorizeApi;
        }

        public async Task<ApiResponseDto<LoginViewModel>> BuildLoginViewModel(string returnUrl)
        {
            return await _authorizeApi.BuildLoginViewModel(returnUrl);
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Login(LoginInputModel parameters)
        {
            ApiResponseDto<LoginResponseModel> apiResponse = await _authorizeApi.Login(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> LoginWith2fa(LoginWith2faInputModel parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.LoginWith2fa(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }
        public async Task<ApiResponseDto> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.LoginWithRecoveryCode(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> Logout()
        {
            ApiResponseDto apiResponse = await _authorizeApi.Logout();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Register(RegisterDto parameters)
        {
            ApiResponseDto<LoginResponseModel> apiResponse = await _authorizeApi.Register(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> Create(RegisterDto parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.Create(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailDto parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ConfirmEmail(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordDto parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ResetPassword(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> UpdatePassword(UpdatePasswordDto parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.UpdatePassword(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordDto parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ForgotPassword(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto<UserInfo>> EnableAuthenticator(AuthenticatorVerificationCodeDto parameters)
        {
            return await _authorizeApi.EnableAuthenticator(parameters);
        }

        public async Task<ApiResponseDto<UserInfo>> DisableAuthenticator()
        {
            return await _authorizeApi.DisableAuthenticator();
        }

        public async Task<ApiResponseDto<UserInfo>> ForgetTwoFactorClient()
        {
            return await _authorizeApi.ForgetTwoFactorClient();
        }

        public async Task<ApiResponseDto<UserInfo>> Enable2fa()
        {
            return await _authorizeApi.Enable2fa();
        }
        public async Task<ApiResponseDto<UserInfo>> Disable2fa()
        {
            return await _authorizeApi.Disable2fa();
        }

        public async Task<UserInfo> GetUserInfo()
        {
            UserInfo userInfo = await _authorizeApi.GetUser();
            bool IsAuthenticated = userInfo.IsAuthenticated;
            if (IsAuthenticated)
            {
                userInfo = await _authorizeApi.GetUserInfo();
            }
            else
            {
                userInfo = new UserInfo { IsAuthenticated = false, Roles = new List<string>() };
            }
            return userInfo;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await GetUserInfo();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, userInfo.UserName) }.Concat(userInfo.ExposedClaims.Select(c => new Claim(c.Key, c.Value)));
                    identity = new ClaimsIdentity(claims, "Server authentication", "name", "role");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex.ToString());
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfo userInfo)
        {
            ApiResponseDto apiResponse = await _authorizeApi.UpdateUser(userInfo);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }
    }
}