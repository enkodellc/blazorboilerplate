using BlazorBoilerplate.Shared.Localizer;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplateMaui;

public partial class App : Application
{
    public App(ILocalizationProvider localizationProvider,
        ILogger<App> logger)
    {
        InitializeComponent();

        MainPage = new LoadingPage(localizationProvider, logger);
    }

#if ANDROID
    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Destroying += (s, e) =>
        {
            Quit();
        };

        return window;
    }
#endif

}
