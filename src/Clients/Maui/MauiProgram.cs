using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplate.Shared.Providers;
using BlazorBoilerplate.Shared.Services;
using BlazorBoilerplate.Theme.Material.Services;
using BlazorBoilerplateMaui.Services;
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
    public static string ServerAddres { get; private set; }
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
        ServerAddres = "https://home.quarella.net";

        builder.Services.AddBlazorWebViewDeveloperTools();

        Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext()
             .WriteTo.Debug()
             .CreateLogger();

        builder.Services.AddLogging(logging =>
        {
            logging.AddSerilog(dispose: true);
        });

        builder.Services.AddLogging();
#else
        ServerAddres = "https://www.blazorboilerplate.com";
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
        builder.Services.AddSingleton(sp => new HttpClient(sp.GetRequiredService<RefreshTokenHandler>()) { BaseAddress = new Uri(ServerAddres) });

        builder.Services.AddAuthorizationCore();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, SharedAuthorizationPolicyProvider>();
        builder.Services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, EmailVerifiedHandler>();
        builder.Services.AddScoped<AuthenticationStateProvider, OidcAuthenticationStateProvider>();

        builder.Services.AddScoped<ILocalizationApiClient, LocalizationApiClient>();
        builder.Services.AddScoped<IAccountApiClient, AccountApiClient>();
        builder.Services.AddScoped<AppState>();
        builder.Services.AddScoped<IApiClient, ApiClient>();
        builder.Services.AddScoped<HubClient>();

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

        builder.Services.RegisterIntlTelInput();

        builder.Services.AddSingleton<ITokenStorage, TokenStorage>();
        builder.Services.AddSingleton(new OidcClient(new()
        {
            Authority = ServerAddres,            
            ClientId = "com.blazorboilerplate.app",
            ClientSecret= "secret",
            Scope = "openid profile email LocalAPI",
            RedirectUri = "com.blazorboilerplate.app://callback",
            PostLogoutRedirectUri = "com.blazorboilerplate.app://callback",

            Browser = new MauiAuthenticationBrowser()
        }));
        builder.Services.AddSingleton<RefreshTokenHandler>();

        return builder.Build();
    }
}
