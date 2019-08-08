using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Shared;

namespace BlazorBoilerplate.Client.Services.Implementations
{
    public class AuthorizeApi : IAuthorizeApi
    {
        private readonly HttpClient _httpClient;

        public AuthorizeApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ClientApiResponse> Login(LoginParameters loginParameters)
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/Authorize/Login", loginParameters);
        }

        public async Task<ClientApiResponse> Logout()
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/Authorize/Logout", null);
        }

        public async Task<ClientApiResponse> Register(RegisterParameters registerParameters)
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/Authorize/Register", registerParameters);
        }

        public async Task<ClientApiResponse> ConfirmEmail(ConfirmEmailParameters confirmEmailParameters)
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/Authorize/ConfirmEmail", confirmEmailParameters);
        }

        public async Task<ClientApiResponse> ResetPassword(ResetPasswordParameters resetPasswordParameters)
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/Authorize/ResetPassword", resetPasswordParameters);
        }

        public async Task<ClientApiResponse> ForgotPassword(ForgotPasswordParameters forgotPasswordParameters)
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/Authorize/ForgotPassword", forgotPasswordParameters);
        }

        public async Task<UserInfo> GetUserInfo()
        {
            UserInfo userInfo = new UserInfo { IsAuthenticated = false, Roles = new String[] { } };
            ClientApiResponse apiResponse = await _httpClient.GetJsonAsync<ClientApiResponse>("api/Authorize/UserInfo");
            
            if (apiResponse.StatusCode == 200)
            {
                userInfo = JsonConvert.DeserializeObject<UserInfo>(apiResponse.Result.ToString());
                return userInfo;
            }
            return userInfo;
        }

        public async Task<ClientApiResponse> UpdateUser(UserInfo userInfo)
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/Authorize/UpdateUser", userInfo);
        }
    }
}
