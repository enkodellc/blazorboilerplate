using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Shared;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorBoilerplate.Client.States
{
    public class BlazorAuthenticationState
    {
        private readonly IAuthorizeApi                     _authorizeApi;
        private readonly ServerAuthenticationStateProvider _serverAuthenticationStateProvider;

        private UserInfo userInfo;

        public BlazorAuthenticationState(IAuthorizeApi authorizeApi,
            ServerAuthenticationStateProvider serverAuthenticationStateProvider)
        {
            _authorizeApi                      = authorizeApi;
            _serverAuthenticationStateProvider = serverAuthenticationStateProvider;
        }

        public async Task<bool> IsLoggedIn()
        {
            try
            {
                var localUserInfo = await GetUserInfo();
                // var userInfo = await GetUserInfo();
                return localUserInfo != null;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task Login(LoginParameters loginParameters)
        {
            userInfo = await _authorizeApi.Login(loginParameters);
            _serverAuthenticationStateProvider.RaiseChangeEvent();

            Console.WriteLine($"Login... userInfo: {userInfo}, userInfo.Username: {userInfo?.Username}");
        }

        public async Task Register(RegisterParameters registerParameters)
        {
            userInfo = await _authorizeApi.Register(registerParameters);
        }

        public async Task Logout()
        {
            Console.WriteLine("Logout...");

            await _authorizeApi.Logout();
            userInfo = null;
            _serverAuthenticationStateProvider.RaiseChangeEvent();

            Console.WriteLine($"Logout... userInfo: {userInfo}, userInfo.Username: {userInfo?.Username}");
        }

        public async Task<UserInfo> GetUserInfo()
        {
            Console.WriteLine("GetUserInfo");
            if (userInfo != null)
            {
                Console.WriteLine("GetUserInfo -> not null");
                Console.WriteLine($"GetUserInfo -> not null -> username: {userInfo.Username}");
                return userInfo;
            }

            Console.WriteLine("GetUserInfo -> null -> get from server");
            userInfo = await _authorizeApi.GetUserInfo();
            return userInfo;
        }
    }
}