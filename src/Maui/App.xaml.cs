using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplateMaui;

public partial class App : Application
{
	public App(ILocalizationApiClient localizationApiClient,
        ILocalizationProvider localizationProvider,
        ILogger<App> logger)
	{
        InitializeComponent();

        MainPage = new LoadingPage(localizationApiClient, localizationProvider, logger);
    }
}
