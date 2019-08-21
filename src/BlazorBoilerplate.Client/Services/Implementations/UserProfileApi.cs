using BlazorBoilerplate.Client.Services.Contracts;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Client.Services.Implementations
{
    public class UserProfileApi : IUserProfileApi
    {
        private readonly HttpClient _httpClient;

        public UserProfileApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponseDto> Get()
        {
            return await _httpClient.GetJsonAsync<ApiResponseDto>("api/UserProfile/Get");
        }

        public async Task<ApiResponseDto> Upsert(UserProfileDto userProfile)
        {
            return await _httpClient.PostJsonAsync<ApiResponseDto>("api/UserProfile/Upsert", userProfile);
        }
    }
}
