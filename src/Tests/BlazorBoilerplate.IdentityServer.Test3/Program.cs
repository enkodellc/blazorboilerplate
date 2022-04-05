using IdentityModel.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorBoilerplate.IdentityServer.Test3
{
    class Program
    {
        static string _authority = "http://localhost:53414";
        private static async Task Main()
        {
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

            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri($"{_authority}/chathub"), options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(tokenResponse.AccessToken);
                })
                .WithAutomaticReconnect()
                .Build();

            try
            {
                connection.On<int, string, string>("ReceiveMessage", (id, username, message) =>
                {
                    Console.WriteLine($"ReceiveMessage: id:{id} username:{username} message:{message}");
                });

                await connection.StartAsync();

                await connection.InvokeAsync("SendMessage", "Ciao!");

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();

                if (connection.State != HubConnectionState.Disconnected)
                {
                    await connection.StopAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
