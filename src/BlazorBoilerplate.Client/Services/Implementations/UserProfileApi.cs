using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BlazorBoilerplate.Shared;
using BlazorBoilerplate.Client.Services.Contracts;

namespace BlazorBoilerplate.Client.Services.Implementations
{
    public class UserProfileApi : IUserProfileApi
    {
        private readonly HttpClient _httpClient;

        public UserProfileApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ClientApiResponse> Get()
        {
            return await _httpClient.GetJsonAsync<ClientApiResponse>("api/UserProfile/Get");
        }

        public async Task<ClientApiResponse> Upsert(UserProfile userProfile)
        {
            return await _httpClient.PostJsonAsync<ClientApiResponse>("api/UserProfile/Upsert", userProfile);
        }
    }
}
