using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Extensions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.Theme.Material.Main.Shared.Components;
using BlazorBoilerplate.Theme.Material.Services;
using BlazorBoilerplate.Theme.Root;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;

namespace BlazorBoilerplate.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
            builder.Services.AddTextLocalization(options =>
            {
                options.ReturnOnlyKeyIfNotFound = !builder.HostEnvironment.IsDevelopment();
                options.FallBackNeutralCulture = !builder.HostEnvironment.IsDevelopment();
            });
            builder.Services.AddDataProtection().SetApplicationName(nameof(BlazorBoilerplate));
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, SharedAuthorizationPolicyProvider>();
            builder.Services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
            builder.Services.AddTransient<IAuthorizationHandler, EmailVerifiedHandler>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();

            builder.Services.AddScoped<ILocalizationApiClient, LocalizationApiClient>();
            builder.Services.AddScoped<IAccountApiClient, AccountApiClient>();
            builder.Services.AddScoped<AppState>();
            builder.Services.AddScoped<IApiClient, ApiClient>();

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

                config.SnackbarConfiguration.PreventDuplicates = true;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            builder.Services.AddScoped<IViewNotifier, ViewNotifier>();

            builder.Services.AddSingleton<IDynamicComponent, NavMenu>();
            builder.Services.AddSingleton<IDynamicComponent, Footer>();
            builder.Services.AddSingleton<IDynamicComponent, DrawerFooter>();
            builder.Services.AddSingleton<IDynamicComponent, TopRightBarSection>();

            builder.Services.RegisterIntlTelInput();

            var host = builder.Build();

            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var js = serviceScope.ServiceProvider.GetService<IJSRuntime>();
                var cookieCulture = await js.GetAspNetCoreCultureCookie();

                if (!string.IsNullOrWhiteSpace(cookieCulture))
                {
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cookieCulture);
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;
                }

                var localizationApiClient = serviceScope.ServiceProvider.GetService<ILocalizationApiClient>();

                var localizationProvider = serviceScope.ServiceProvider.GetService<ILocalizationProvider>();

                var localizationRecordsTask = localizationApiClient.GetLocalizationRecords();

                var pluralFormRulesTask = localizationApiClient.GetPluralFormRules();

                await Task.WhenAll(new Task[] { localizationRecordsTask, pluralFormRulesTask });

                localizationProvider.Init(localizationRecordsTask.Result, pluralFormRulesTask.Result);
            }

            await host.RunAsync();
        }
    }
}
