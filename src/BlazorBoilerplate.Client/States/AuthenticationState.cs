using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Shared;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Client.States
{
    public class AuthenticationState
    {
        private readonly IAuthorizeApi _authorizeApi;
        private UserInfo userInfo;

        public AuthenticationState(IAuthorizeApi authorizeApi)
        {
            _authorizeApi = authorizeApi;
        }

        public async Task<bool> IsLoggedIn()
        {
            try
            {
                var userInfo = await GetUserInfo();
                return userInfo != null;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task Login(LoginParameters loginParameters)
        {
            userInfo = await _authorizeApi.Login(loginParameters);
        }

        public async Task Register(RegisterParameters registerParameters)
        {
            userInfo = await _authorizeApi.Register(registerParameters);
        }

        public async Task Logout()
        {
            await _authorizeApi.Logout();
            userInfo = null;
        }

        public async Task<UserInfo> GetUserInfo()
        {
            if (userInfo != null) return userInfo;
            userInfo = await _authorizeApi.GetUserInfo();
            return userInfo;
        }

    }
}
