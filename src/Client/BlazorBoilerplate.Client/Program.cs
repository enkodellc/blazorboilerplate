using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorBoilerplate.Extensions;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using BlazorLazyLoading.Wasm;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.ConfigureContainer(new DryIocServiceProviderFactory()); // allow runtime registrations (required for lazy module loads)

            AddHttpClient(builder.Services, builder);

            builder.Services.AddLazyLoading(new LazyLoadingOptions
            {
                ModuleHints = new[] { "BlazorBoilerplate.Modules" },
            });

            await builder.Services.BuildServiceProvider().LoadModules();

            builder.Services.AddLocalization();
            builder.Services.AddDataProtection().SetApplicationName(nameof(BlazorBoilerplate));
            builder.Services.AddProtectedBrowserStorage();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddAuthorizationCore(config =>
            {
                config.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
                config.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
                config.AddPolicy(Policies.IsReadOnly, Policies.IsUserPolicy());
                // config.AddPolicy(Policies.IsMyDomain, Policies.IsMyDomainPolicy());  Only works on the server end,
            });

            builder.Services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
            builder.Services.Add(new ServiceDescriptor(typeof(IUserProfileApi), typeof(UserProfileApi), ServiceLifetime.Scoped));
            builder.Services.AddScoped<AppState>();

            builder.ConfigureModules();

            var host = builder.Build();

            host.Services.InitializeModules(host);
            await host.RunAsync();
        }

        private static void AddHttpClient(IServiceCollection services, WebAssemblyHostBuilder builder)
        {
            services.AddSingleton(
                typeof(HttpClient),
                p => new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                });
        }
    }
}
