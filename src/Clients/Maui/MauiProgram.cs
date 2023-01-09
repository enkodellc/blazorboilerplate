using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.Theme.Material.Main.Shared.Components;
using BlazorBoilerplate.Theme.Material.Services;
using IdentityModel;
using IdentityModel.OidcClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor;
using MudBlazor.Services;
#if DEBUG
using Serilog;
#endif

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
        string baseAddress = "https://openid.quarella.net";

        builder.Services.AddBlazorWebViewDeveloperTools();

        Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext()
             .WriteTo.Debug()
             .WriteTo.File(path: @"D:\maui.log")
             .CreateLogger();

        builder.Services.AddLogging(logging =>
        {
            logging.AddSerilog(dispose: true);
        });

        builder.Services.AddLogging();
#else
        string baseAddress = "https://www.blazorboilerplate.com";
#endif

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

        builder.Services.AddSingleton(new OidcClient(new()
        {
            Authority = baseAddress,

            ClientId = "myapp",
            ClientSecret= "secret",
            Scope = "openid profile LocalAPI",
            RedirectUri = "myapp://callback",

            Browser = new MauiAuthenticationBrowser()
        }));

        return builder.Build();
    }
}
