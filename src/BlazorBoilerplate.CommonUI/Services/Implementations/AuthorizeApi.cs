using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using BlazorBoilerplate.CommonUI.Services.Contracts;
using BlazorBoilerplate.Shared.Dto;
using System.Collections.Generic;
using BlazorBoilerplate.Shared.Dto.Account;
using Microsoft.JSInterop;
using System.Linq;
using System.Net;

using static Microsoft.AspNetCore.Http.StatusCodes;
using System.Net.Http.Json;

namespace BlazorBoilerplate.CommonUI.Services.Implementations
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

        public async Task<ApiResponseDto> Login(LoginDto loginParameters)
        {
            ApiResponseDto resp;

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Account/Login", loginParameters);
            using (response)
            {
                response.EnsureSuccessStatusCode();

#if ServerSideBlazor

                if (response.Headers.TryGetValues("Set-Cookie", out var cookieEntries))
                {
                    var uri = response.RequestMessage.RequestUri;
                    var cookieContainer = new CookieContainer();

                    foreach (var cookieEntry in cookieEntries)
                    {
                        cookieContainer.SetCookies(uri, cookieEntry);
                    }

                    var cookies = cookieContainer.GetCookies(uri).Cast<Cookie>();

                    foreach (var cookie in cookies)
                    {
                       await _jsRuntime.InvokeVoidAsync("jsInterops.setCookie", cookie.ToString());
                    }
                }
#endif

                var content = await response.Content.ReadAsStringAsync();
                resp = JsonConvert.DeserializeObject<ApiResponseDto>(content);
            }

            return resp;
        }

        public async Task<ApiResponseDto> Logout()
        {
#if ServerSideBlazor
            List<string> cookies = null;
            if (_httpClient.DefaultRequestHeaders.TryGetValues("Cookie", out IEnumerable<string> cookieEntries))
                cookies = cookieEntries.ToList();
#endif

            HttpResponseMessage response = await _httpClient.GetAsync("api/Account/Logout");
            ApiResponseDto apiResponse = JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());

#if ServerSideBlazor
            if (apiResponse.StatusCode == Status200OK  && cookies != null && cookies.Any())
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");

                foreach (var cookie in cookies[0].Split(';'))
                {
                    var cookieParts = cookie.Split('=');
                    await _jsRuntime.InvokeVoidAsync("jsInterops.removeCookie", cookieParts[0]);
                }
            }
#endif

            return apiResponse;
        }

        public async Task<ApiResponseDto> Create(RegisterDto registerParameters)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Account/Create", registerParameters);
            return JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ApiResponseDto> Register(RegisterDto registerParameters)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Account/Register", registerParameters);
            return JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ApiResponseDto> ConfirmEmail(ConfirmEmailDto confirmEmailParameters)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Account/ConfirmEmail", confirmEmailParameters);
            return JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ApiResponseDto> ResetPassword(ResetPasswordDto resetPasswordParameters)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Account/ResetPassword", resetPasswordParameters);
            return JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ApiResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordParameters)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Account/ForgotPassword", forgotPasswordParameters);
            return JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<UserInfoDto> GetUserInfo()
        {
            UserInfoDto userInfo = new UserInfoDto { IsAuthenticated = false, Roles = new List<string>() };

            HttpResponseMessage response = await _httpClient.GetAsync("api/Account/UserInfo");
            ApiResponseDto apiResponse = JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());

            if (apiResponse.StatusCode == Status200OK)
            {
                userInfo = JsonConvert.DeserializeObject<UserInfoDto>(apiResponse.Result.ToString());
                return userInfo;
            }
            return userInfo;
        }

        public async Task<UserInfoDto> GetUser()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/Account/GetUser");
            ApiResponseDto apiResponse = JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());
            UserInfoDto user = JsonConvert.DeserializeObject<UserInfoDto>(apiResponse.Result.ToString());
            return user;
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfoDto userInfo)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Account/UpdateUser", userInfo);
            return JsonConvert.DeserializeObject<ApiResponseDto>(await response.Content.ReadAsStringAsync());
        }
    }
}
