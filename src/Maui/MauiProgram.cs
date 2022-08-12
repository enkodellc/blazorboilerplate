using Microsoft.AspNetCore.Authorization;
using Microsoft.JSInterop;
using System.Globalization;
using System.Reflection;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Localizer;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Extensions;
using Microsoft.AspNetCore.DataProtection;

namespace BlazorBoilerplateMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        string baseAddress = "http://localhost:53414";
        builder.Services.AddBlazorWebViewDeveloperTools();
#else
        string baseAddress = "https://www.blazorboilerplate.com";
#endif

        var baseModule = new BlazorBoilerplate.Theme.Material.Main.Module();
        var demoModule = new BlazorBoilerplate.Theme.Material.Admin.Module();

        Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        ModuleProvider.Init(allAssemblies);

        //builder.RootComponents.AddRange(new[] { ModuleProvider.RootComponentMapping });

        builder.Services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
        builder.Services.AddTextLocalization(options =>
        {
#if DEBUG
            options.ReturnOnlyKeyIfNotFound = false;
            options.FallBackNeutralCulture = false;
#else
            options.ReturnOnlyKeyIfNotFound = true;
            options.FallBackNeutralCulture = true;
#endif
        });
        builder.Services.AddDataProtection().SetApplicationName(nameof(BlazorBoilerplate));
        builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

        builder.Services.AddAuthorizationCore();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, SharedAuthorizationPolicyProvider>();
        builder.Services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, EmailVerifiedHandler>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();

        builder.Services.AddScoped<ILocalizationApiClient, LocalizationApiClient>();
        builder.Services.AddScoped<IAccountApiClient, AccountApiClient>();
        builder.Services.AddScoped<AppState>();
        builder.Services.AddScoped<IApiClient, ApiClient>();

        foreach (var module in ModuleProvider.Modules)
            module.ConfigureWebAssemblyServices(builder.Services);

        var host = builder.Build();

        //foreach (var module in ModuleProvider.Modules)
        //    module.ConfigureWebAssemblyHost(host);

        using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var js = serviceScope.ServiceProvider.GetService<IJSRuntime>();
            var cookieCulture = js.GetAspNetCoreCultureCookie().Result;

            if (!string.IsNullOrWhiteSpace(cookieCulture))
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cookieCulture);
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;
            }

            var localizationApiClient = serviceScope.ServiceProvider.GetService<ILocalizationApiClient>();

            var localizationProvider = serviceScope.ServiceProvider.GetService<ILocalizationProvider>();

            var localizationRecordsTask = localizationApiClient.GetLocalizationRecords();

            var pluralFormRulesTask = localizationApiClient.GetPluralFormRules();

            localizationProvider.Init(localizationRecordsTask.Result, pluralFormRulesTask.Result);
        }

        return host;
    }
}
