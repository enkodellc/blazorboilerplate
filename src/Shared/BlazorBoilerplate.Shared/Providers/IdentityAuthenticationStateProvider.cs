using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Models.Account;

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

        public async Task<ApiResponseDto<LoginResponseModel>> Register(RegisterViewModel parameters)
        {
            ApiResponseDto<LoginResponseModel> apiResponse = await _authorizeApi.Register(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> Create(RegisterViewModel parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.Create(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailViewModel parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ConfirmEmail(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordViewModel parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ResetPassword(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> UpdatePassword(UpdatePasswordViewModel parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.UpdatePassword(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordViewModel parameters)
        {
            ApiResponseDto apiResponse = await _authorizeApi.ForgotPassword(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto<UserViewModel>> EnableAuthenticator(AuthenticatorVerificationCodeViewModel parameters)
        {
            return await _authorizeApi.EnableAuthenticator(parameters);
        }

        public async Task<ApiResponseDto<UserViewModel>> DisableAuthenticator()
        {
            return await _authorizeApi.DisableAuthenticator();
        }

        public async Task<ApiResponseDto<UserViewModel>> ForgetTwoFactorClient()
        {
            return await _authorizeApi.ForgetTwoFactorClient();
        }

        public async Task<ApiResponseDto<UserViewModel>> Enable2fa()
        {
            return await _authorizeApi.Enable2fa();
        }
        public async Task<ApiResponseDto<UserViewModel>> Disable2fa()
        {
            return await _authorizeApi.Disable2fa();
        }

        public async Task<UserViewModel> GetUserViewModel()
        {
            UserViewModel userViewModel = await _authorizeApi.GetUser();
            bool IsAuthenticated = userViewModel.IsAuthenticated;
            if (IsAuthenticated)
            {
                userViewModel = await _authorizeApi.GetUserViewModel();
            }
            else
            {
                userViewModel = new UserViewModel { IsAuthenticated = false, Roles = new List<string>() };
            }
            return userViewModel;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userViewModel = await GetUserViewModel();
                if (userViewModel.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, userViewModel.UserName) }.Concat(userViewModel.ExposedClaims.Select(c => new Claim(c.Key, c.Value)));
                    identity = new ClaimsIdentity(claims, "Server authentication", "name", "role");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex.ToString());
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<ApiResponseDto> UpdateUser(UserViewModel userViewModel)
        {
            ApiResponseDto apiResponse = await _authorizeApi.UpdateUser(userViewModel);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }
    }
}