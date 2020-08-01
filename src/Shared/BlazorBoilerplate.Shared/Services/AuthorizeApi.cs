using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Account;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Services
{
    public class AuthorizeApi : IAuthorizeApi
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public AuthorizeApi(NavigationManager navigationManager, HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponseDto<LoginViewModel>> BuildLoginViewModel(string returnUrl)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<LoginViewModel>>("api/Account/BuildLoginViewModel", returnUrl);
        }

        private async Task SubmitServerForm(string path, AccountFormModel model)
        {
            model.__RequestVerificationToken = await _jsRuntime.InvokeAsync<string>("interop.getElementByName", "__RequestVerificationToken");

            await _jsRuntime.InvokeAsync<string>("interop.submitForm", path, model);
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Login(LoginInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto<LoginResponseModel>>("api/Account/Login", parameters);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/login/", parameters);

            return response;
        }

        public async Task<ApiResponseDto> LoginWith2fa(LoginWith2faInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/LoginWith2fa", parameters);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/loginwith2fa/", parameters);

            return response;
        }
        public async Task<ApiResponseDto> LoginWithRecoveryCode(LoginWithRecoveryCodeInputModel parameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/LoginWithRecoveryCode", parameters);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/loginwith2fa/", parameters);

            return response;
        }

        public async Task<ApiResponseDto> Logout()
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Logout", null);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/logout/", new AccountFormModel());

            return response;
        }

        public async Task<ApiResponseDto> Create(RegisterDto parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Create", parameters);
        }

        public async Task<ApiResponseDto<LoginResponseModel>> Register(RegisterDto parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<LoginResponseModel>>("api/Account/Register", parameters);
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailDto parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ConfirmEmail", parameters);
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordDto parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ResetPassword", parameters);
        }

        public async Task<ApiResponseDto> UpdatePassword(UpdatePasswordDto parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdatePassword", parameters);
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordDto parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ForgotPassword", parameters);
        }

        public async Task<ApiResponseDto<UserInfo>> EnableAuthenticator(AuthenticatorVerificationCodeDto parameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserInfo>>("api/Account/EnableAuthenticator", parameters);
        }
        public async Task<ApiResponseDto<UserInfo>> DisableAuthenticator()
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserInfo>>("api/Account/DisableAuthenticator", null);
        }
        public async Task<ApiResponseDto<UserInfo>> ForgetTwoFactorClient()
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto<UserInfo>>("api/Account/ForgetTwoFactorClient", null);

            if (!_navigationManager.IsWebAssembly())
                if (response.IsSuccessStatusCode)
                    await SubmitServerForm("/server/ForgetTwoFactorClient/", new AccountFormModel());

            return response;
        }
        public async Task<ApiResponseDto<UserInfo>> Enable2fa()
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserInfo>>("api/Account/Enable2fa", null);
        }
        public async Task<ApiResponseDto<UserInfo>> Disable2fa()
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto<UserInfo>>("api/Account/Disable2fa", null);
        }

        public async Task<UserInfo> GetUserInfo()
        {
            UserInfo userInfo = new UserInfo { IsAuthenticated = false, Roles = new List<string>() };

            var apiResponse = await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserInfo>>("api/Account/UserInfo");

            if (apiResponse.IsSuccessStatusCode)
                userInfo = apiResponse.Result;

            return userInfo;
        }

        public async Task<UserInfo> GetUser()
        {
            var apiResponse = await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserInfo>>("api/Account/GetUser");
            return apiResponse.Result;
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfo userInfo)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdateUser", userInfo);
        }
    }
}
