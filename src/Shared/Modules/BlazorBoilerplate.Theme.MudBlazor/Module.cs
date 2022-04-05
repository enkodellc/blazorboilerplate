using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Theme.Material.Services;
using BlazorBoilerplate.Theme.Material.TagHelpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Theme.Material
{
    public class Module : IModule, ITheme
    {
        public static readonly string ContentPath = $"_content/{typeof(Module).Namespace.Replace("Material", "MudBlazor")}";
        public static readonly string Path = typeof(Module).Namespace.Replace("Material", "MudBlazor");
        public Module()
        {
            RootComponentMapping = new RootComponentMapping(typeof(App), "app");
        }

        public RootComponentMapping RootComponentMapping { get; }

        public string Name => "MudBlazor theme";

        public string Description => "MudBlazor theme";

        public int Order => 1;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITagHelperComponent, ThemeTagHelperComponent>();
            services.AddTransient<ITagHelperComponent, AppTagHelperComponent>();

            services.AddMudServices(config =>
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

            services.AddScoped<IViewNotifier, ViewNotifier>();
        }

        public void ConfigureWebAssemblyServices(IServiceCollection services)
        {
            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            services.AddLoadingBar();

            services.AddScoped<IViewNotifier, ViewNotifier>();

            var sp = services.BuildServiceProvider();

            sp.GetRequiredService<HttpClient>().EnableIntercept(sp);
        }

        public void ConfigureWebAssemblyHost(WebAssemblyHost webAssemblyHost)
        {
            webAssemblyHost.UseLoadingBar();
        }
    }
}
