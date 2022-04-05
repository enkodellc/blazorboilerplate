using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models.Account;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorBoilerplate.Shared.Providers
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IAccountApiClient _accountApiClient;

        public IdentityAuthenticationStateProvider(IAccountApiClient accountApiClient)
        {
            _accountApiClient = accountApiClient;
        }

        public async Task<ApiResponseDto<LoginViewModel>> BuildLoginViewModel(string returnUrl)
        {
            return await _accountApiClient.BuildLoginViewModel(returnUrl);
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Login(LoginInputModel parameters)
        {
            ApiResponseDto<LoginResponseModel> apiResponse = await _accountApiClient.Login(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto> LoginWith2fa(LoginWith2faInputModel parameters)
        {
            ApiResponseDto apiResponse = await _accountApiClient.LoginWith2fa(parameters);
            return apiResponse;
        }
        public async Task<ApiResponseDto> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters)
        {
            ApiResponseDto apiResponse = await _accountApiClient.LoginWithRecoveryCode(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto> Logout(string returnUrl = null)
        {
            ApiResponseDto apiResponse = await _accountApiClient.Logout(returnUrl);
            return apiResponse;
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Register(RegisterViewModel parameters)
        {
            ApiResponseDto<LoginResponseModel> apiResponse = await _accountApiClient.Register(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto> Create(RegisterViewModel parameters)
        {
            return await _accountApiClient.Create(parameters);
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailViewModel parameters)
        {
            ApiResponseDto apiResponse = await _accountApiClient.ConfirmEmail(parameters);
            return apiResponse;
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordViewModel parameters)
        {
            ApiResponseDto apiResponse = await _accountApiClient.ResetPassword(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> UpdatePassword(UpdatePasswordViewModel parameters)
        {
            ApiResponseDto apiResponse = await _accountApiClient.UpdatePassword(parameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> AdminChangePassword(ChangePasswordViewModel parameters)
        {
            return await _accountApiClient.AdminChangePassword(parameters);
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordViewModel parameters)
        {
            return await _accountApiClient.ForgotPassword(parameters);
        }

        public async Task<ApiResponseDto<UserViewModel>> EnableAuthenticator(AuthenticatorVerificationCodeViewModel parameters)
        {
            return await _accountApiClient.EnableAuthenticator(parameters);
        }

        public async Task<ApiResponseDto<UserViewModel>> DisableAuthenticator()
        {
            return await _accountApiClient.DisableAuthenticator();
        }

        public async Task<ApiResponseDto<UserViewModel>> ForgetTwoFactorClient()
        {
            return await _accountApiClient.ForgetTwoFactorClient();
        }

        public async Task<ApiResponseDto<UserViewModel>> Enable2fa()
        {
            return await _accountApiClient.Enable2fa();
        }
        public async Task<ApiResponseDto<UserViewModel>> Disable2fa()
        {
            return await _accountApiClient.Disable2fa();
        }

        public async Task<UserViewModel> GetUserViewModel()
        {
            UserViewModel userViewModel = await _accountApiClient.GetUser();
            bool IsAuthenticated = userViewModel.IsAuthenticated;

            if (IsAuthenticated)
                userViewModel = await _accountApiClient.GetUserViewModel();
            else
                userViewModel = new UserViewModel { IsAuthenticated = false, Roles = new List<string>() };

            return userViewModel;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();

            var userViewModel = await GetUserViewModel();

            if (userViewModel.IsAuthenticated)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, userViewModel.UserName) }.Concat(userViewModel.ExposedClaims.Select(c => new Claim(c.Key, c.Value)));
                identity = new ClaimsIdentity(claims, "Server authentication", "name", "role");
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<ApiResponseDto> UpdateUser(UserViewModel userViewModel)
        {
            ApiResponseDto apiResponse = await _accountApiClient.UpdateUser(userViewModel);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponseDto> AdminUpdateUser(UserViewModel userViewModel)
        {
            return await _accountApiClient.AdminUpdateUser(userViewModel);
        }
    }
}