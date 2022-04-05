using BlazorBoilerplate.Shared.Services;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BlazorBoilerplate.Server.Tests.Oidc
{
    //https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/tree/main/NetCoreConsoleClient
    class IdentityServerTests
    {
        static string _authority = "http://localhost:53414";

        static OidcClient _oidcClient;
        static HttpClient _httpClient = new HttpClient() { BaseAddress = new Uri(_authority) };

        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static ServiceProvider serviceProvider;
        public static async Task MainAsync()
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

            Console.WriteLine("+-----------------------+");
            Console.WriteLine("|  Sign in with OIDC    |");
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("");
            Console.WriteLine("Press any key to sign in...");
            Console.ReadKey();

            await Login();
        }

        private static async Task Login()
        {
            // create a redirect URI using an available port on the loopback address.
            // requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
            var browser = new SystemBrowser(64879);
            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var options = new OidcClientOptions
            {
                Authority = _authority,
                ClientId = "swaggerui",
                RedirectUri = redirectUri,
                Scope = "openid profile IdentityServerApi",
                FilterClaims = false,
                Browser = browser,
                IdentityTokenValidator = new JwtHandlerIdentityTokenValidator(),
                RefreshTokenInnerHttpHandler = new HttpClientHandler()
            };

            options.Policy.Discovery.ValidateIssuerName = false;
            options.BackchannelHandler = new HttpClientHandler() { ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true };

            options.LoggerFactory.AddSerilog(serviceProvider.GetService<Serilog.ILogger>());

            _oidcClient = new OidcClient(options);
            var result = await _oidcClient.LoginAsync(new LoginRequest());

            ShowResult(result);
            await NextSteps(result);
        }

        private static void ShowResult(LoginResult result)
        {
            if (result.IsError)
            {
                Console.WriteLine("\n\nError:\n{0}", result.Error);
                return;
            }

            Console.WriteLine("\n\nClaims:");
            foreach (var claim in result.User.Claims)
            {
                Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
            }

            Console.WriteLine($"\nidentity token: {result.IdentityToken}");
            Console.WriteLine($"access token:   {result.AccessToken}");
            Console.WriteLine($"refresh token:  {result?.RefreshToken ?? "none"}");
        }

        private static async Task NextSteps(LoginResult result)
        {
            var currentAccessToken = result.AccessToken;
            var currentRefreshToken = result.RefreshToken;

            var menu = "\npress x to exit \npress c to call api GetUserProfile\n";

            if (currentRefreshToken != null)
                menu += "press r to refresh token\n";

            while (true)
            {
                Console.WriteLine("\n");

                Console.Write(menu);
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.X) return;
                if (key.Key == ConsoleKey.C) await CallApi(currentAccessToken);
                if (key.Key == ConsoleKey.R)
                {
                    var refreshResult = await _oidcClient.RefreshTokenAsync(currentRefreshToken);

                    if (refreshResult.IsError)
                    {
                        Console.WriteLine($"Error: {refreshResult.Error}");
                    }
                    else
                    {
                        currentRefreshToken = refreshResult.RefreshToken;
                        currentAccessToken = refreshResult.AccessToken;

                        Console.WriteLine("\n\n");
                        Console.WriteLine($"access token:   {refreshResult.AccessToken}");
                        Console.WriteLine($"refresh token:  {refreshResult?.RefreshToken ?? "none"}");
                    }
                }
            }
        }

        private static async Task CallApi(string currentAccessToken)
        {
            _httpClient.SetBearerToken(currentAccessToken);

            var apiClient = new ApiClient(_httpClient, serviceProvider.GetService<ILogger<ApiClient>>());

            try
            {
                var profile = await apiClient.GetUserProfile();

                Console.WriteLine($"\nuserId profile: {profile.UserId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
