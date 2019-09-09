using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Shared.Dto;
using System.Collections.Generic;

namespace BlazorBoilerplate.Client.Services.Implementations
{
    public class AuthorizeApi : IAuthorizeApi
    {
        private readonly HttpClient _httpClient;

        public AuthorizeApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponseDto> Login(LoginDto loginParameters)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Login", loginParameters);
        }

        public async Task<ApiResponseDto> Logout()
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Logout", null);
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
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Account/UserInfo");
            
            if (apiResponse.StatusCode == 200)
            {
                userInfo = JsonConvert.DeserializeObject<UserInfoDto>(apiResponse.Result.ToString());
                return userInfo;
            }
            return userInfo;
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfoDto userInfo)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdateUser", userInfo);
        }
    }
}
