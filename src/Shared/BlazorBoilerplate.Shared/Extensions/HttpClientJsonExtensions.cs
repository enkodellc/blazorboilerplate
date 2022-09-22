using Newtonsoft.Json;
using System.Text;

namespace BlazorBoilerplate.Shared.Extensions
{
    public static class HttpClientJsonExtensions
    {
        //System.Text.Json does not like HashSet. So I have to use Newtonsoft
        //See https://github.com/dotnet/runtime/issues/31553
        //TODO remove and use GetFromJsonAsync in the future
        public static async Task<T> GetNewtonsoftJsonAsync<T>(this HttpClient httpClient, string requestUri)
        {
            var stringContent = await httpClient.GetStringAsync(requestUri);
            return JsonConvert.DeserializeObject<T>(stringContent);
        }

        public static async Task<T> PostNewtonsoftJsonAsync<T>(this HttpClient httpClient, string requestUri, object content)
        {
            string json = JsonConvert.SerializeObject(content, LocalJsonSerializer.Settings);

            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

            var stringContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(stringContent, null, response.StatusCode);

            return JsonConvert.DeserializeObject<T>(stringContent);
        }

        internal static class LocalJsonSerializer
        {
            public static readonly JsonSerializerSettings Settings = new()
            {
                ContractResolver = new SkipEntityAspectContractResolver()
            };
        }
    }
}

