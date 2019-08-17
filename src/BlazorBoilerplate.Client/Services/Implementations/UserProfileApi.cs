using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using BlazorBoilerplate.Shared.Dto;
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
