using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Globalization;
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

            //TODO see what oqtane does
            var baseModule = new Theme.Material.Module();
            var adminModule = new Theme.Material.Admin.Module();
            var demoModule = new Theme.Material.Demo.Module();

            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            ModuleProvider.Init(allAssemblies);

            builder.RootComponents.AddRange(new[] { ModuleProvider.RootComponentMapping });

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
            builder.Services.AddScoped<AppState>();
            builder.Services.AddTransient<IApiClient, ApiClient>();

            foreach (var module in ModuleProvider.Modules)
                module.ConfigureWebAssemblyServices(builder.Services);

            var host = builder.Build();

            foreach (var module in ModuleProvider.Modules)
                module.ConfigureWebAssemblyHost(host);

            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var js = serviceScope.ServiceProvider.GetService<IJSRuntime>();
                var cookieCulture = await js.GetAspNetCoreCultureCookie();

                if (!string.IsNullOrWhiteSpace(cookieCulture))
                {
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cookieCulture);
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;
                }
            }            

            await host.RunAsync();
        }
    }
}
