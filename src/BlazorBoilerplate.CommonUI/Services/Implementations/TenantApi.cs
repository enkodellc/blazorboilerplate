using BlazorBoilerplate.CommonUI.Services.Contracts;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.Services.Implementations
{
    public class TenantApi : ITenantApi
    {
        public Tenant Tenant { get; set; } = new Tenant();
        private readonly HttpClient _httpClient;

        public TenantApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Tenant> GetTenant()
        {
            ApiResponseDto apiResponse = await _httpClient.GetJsonAsync<ApiResponseDto>("api/Tenants/Current");
            if (apiResponse.Result != null)
            {
                Tenant = JsonConvert.DeserializeObject<Tenant>(apiResponse.Result.ToString());
            }
            return Tenant;
        }
    }
}