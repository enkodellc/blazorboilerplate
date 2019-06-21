using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorBoilerplate.Client
{
    // https://gist.github.com/SteveSandersonMS/175a08dcdccb384a52ba760122cd2eda

    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public ServerAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfo = await _httpClient.GetJsonAsync<UserInfo>("api/authorize");

            // CLIENT side claims:
            var claims = new List<Claim>();

            //if (userInfo != null && !string.IsNullOrEmpty(userInfo.Name)) 
            //{
            //    claims.Add(new Claim(ClaimTypes.Name, userInfo.Name));

            //    if (userInfo.Name == "nstohler")
            //    {
            //        claims.Add(new Claim("hans", "wurst"));
            //    }
            //    else
            //    {
            //        claims.Add(new Claim("hallo", "velo"));
            //    }
            //}

            var identity = userInfo.IsAuthenticated
                ? new ClaimsIdentity(
                    //claims.ToArray()
                    new[]
                    {
                        new Claim(ClaimTypes.Name, userInfo.Username)
                    }
                    , "serverauth")
                : new ClaimsIdentity();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // TODO: how to do this, required at all???
        public void RaiseChangeEvent()
        {
            var task = GetAuthenticationStateAsync();
            base.NotifyAuthenticationStateChanged(task);
        }
    }
}
