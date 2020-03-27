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

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Account/Login");
            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(loginParameters));
            httpRequestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using (var response = await _httpClient.SendAsync(httpRequestMessage))
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

            var resp = await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/Logout", null);

#if ServerSideBlazor
            if (resp.StatusCode == Status200OK  && cookies != null && cookies.Any())
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");

                foreach (var cookie in cookies[0].Split(';'))
                {
                    var cookieParts = cookie.Split('=');
                    await _jsRuntime.InvokeVoidAsync("jsInterops.removeCookie", cookieParts[0]);
                }
            }
#endif

            return resp;
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

            if (apiResponse.StatusCode == Status200OK)
            {
                userInfo = JsonConvert.DeserializeObject<UserInfoDto>(apiResponse.Result.ToString());
                return userInfo;
            }
            return userInfo;
        }

        public async Task<UserInfoDto> GetUser()
        {
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Account/GetUser");
            UserInfoDto user = JsonConvert.DeserializeObject<UserInfoDto>(apiResponse.Result.ToString());
            return user;
        }

        public async Task<ApiResponseDto> UpdateUser(UserInfoDto userInfo)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/Account/UpdateUser", userInfo);
        }
    }
}
