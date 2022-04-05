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
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(stringContent);
        }
    }
}

