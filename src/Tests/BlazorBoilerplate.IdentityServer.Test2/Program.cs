using BlazorBoilerplate.Shared.Services;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BlazorBoilerplate.IdentityServer.Test2
{
    class Program
    {
        static string _authority = "http://localhost:53414";

        private static ServiceProvider serviceProvider;
        private static async Task Main()
        {
            serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Error()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                .CreateLogger();

            serviceProvider.GetService<ILoggerFactory>().AddSerilog(serilog);

            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(_authority);
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "clientToDo",
                ClientSecret = "secret",

                Scope = "LocalAPI"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var httpClient = new HttpClient() { BaseAddress = new Uri(_authority) };
            httpClient.SetBearerToken(tokenResponse.AccessToken);

            var apiClient = new ApiClient(httpClient, serviceProvider.GetService<ILogger<ApiClient>>());

            try
            {
                var todos = await apiClient.GetToDos(null);

                Console.WriteLine($"\ntodo found: {todos.InlineCount}");

                foreach (var todo in todos.Results)
                {
                    apiClient.RemoveEntity(todo);

                    await apiClient.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
