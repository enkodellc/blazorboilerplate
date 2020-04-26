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

        public async Task<ApiResponseDto> Login(LoginInputModel loginParameters)
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Login", loginParameters);

//-:cnd:noEmit
#if ServerSideBlazor
            if (response.IsSuccessStatusCode)
            {
                loginParameters.__RequestVerificationToken = await _jsRuntime.InvokeAsync<string>("interop.getElementByName", "__RequestVerificationToken");

                await _jsRuntime.InvokeAsync<string>("interop.submitForm", "/server/login/", loginParameters);
            }
#endif
//-:cnd:noEmit

            return response;
        }

        public async Task<ApiResponseDto> Logout()
        {
            var response = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Logout", null);

//-:cnd:noEmit
#if ServerSideBlazor
            if (response.IsSuccessStatusCode)
            {
                var antiforgerytoken = await _jsRuntime.InvokeAsync<string>("interop.getElementByName", "__RequestVerificationToken");
                await _jsRuntime.InvokeAsync<string>("interop.submitForm", "/server/logout/", new { __RequestVerificationToken = antiforgerytoken, returnurl = "" });
            }
#endif
//-:cnd:noEmit

            return response;
        }

        public async Task<ApiResponseDto> Create(RegisterDto registerParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Create", registerParameters);
        }

        public async Task<ApiResponseDto> Register(RegisterDto registerParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Register", registerParameters);
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailDto confirmEmailParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ConfirmEmail", confirmEmailParameters);
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordDto resetPasswordParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ResetPassword", resetPasswordParameters);
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/ForgotPassword", forgotPasswordParameters);
        }

        public async Task<UserInfoDto> GetUserInfo()
        {
            UserInfoDto userInfo = new UserInfoDto { IsAuthenticated = false, Roles = new List<string>() };

            var apiResponse = await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserInfoDto>>("api/Account/UserInfo");

            if (apiResponse.IsSuccessStatusCode)
                userInfo = apiResponse.Result;

            return userInfo;
        }

        public async Task<UserInfoDto> GetUser()
        {
            var apiResponse = await _httpClient.GetNewtonsoftJsonAsync<ApiResponseDto<UserInfoDto>>("api/Account/GetUser");
            return apiResponse.Result;
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfoDto userInfo)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdateUser", userInfo);
        }
    }
}
