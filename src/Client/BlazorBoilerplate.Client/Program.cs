using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            //TODO find a solution to do dinamically, without this hack (download dll modules and load in memory?)
            var baseModule = new BlazorBoilerplate.Theme.Material.Module();
            var adminModule = new BlazorBoilerplate.Theme.Material.Admin.Module();
            var demoModule = new BlazorBoilerplate.Theme.Material.Demo.Module();

            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            ModuleProvider.Init(allAssemblies);

            builder.RootComponents.AddRange(new [] { ModuleProvider.RootComponentMapping });

            builder.Services.AddLocalization();
            builder.Services.AddDataProtection().SetApplicationName(nameof(BlazorBoilerplate));
            builder.Services.AddProtectedBrowserStorage();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddAuthorizationCore(config =>
            {
                config.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
                config.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
                config.AddPolicy(Policies.IsReadOnly, Policies.IsUserPolicy());
                // config.AddPolicy(Policies.IsMyDomain, Policies.IsMyDomainPolicy());  Only works on the server end
            });

            builder.Services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
            builder.Services.Add(new ServiceDescriptor(typeof(IUserProfileApi), typeof(UserProfileApi), ServiceLifetime.Scoped));
            builder.Services.AddScoped<AppState>();
            builder.Services.AddTransient<IApiClient, ApiClient>();

            foreach (var module in ModuleProvider.Modules)
                module.ConfigureWebAssemblyServices(builder.Services);

            var host = builder.Build();

            foreach (var module in ModuleProvider.Modules)
                module.ConfigureWebAssemblyHost(host);

            await host.RunAsync();
        }
    }
}
