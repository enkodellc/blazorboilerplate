using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplateMaui.Resources;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplateMaui;

public partial class LoadingPage : ContentPage
{
    public LoadingPage(
        ILocalizationProvider localizationProvider,
        ILogger<App> logger)
    {
        InitializeComponent();

        Task.Run(async () =>
        {
            try
            {
                var streams = new Dictionary<string, Stream>();

                var stream = await FileSystem.OpenAppPackageFileAsync("Resources\\Raw\\en-US.po");
                streams.Add("en-US", stream);

                stream = await FileSystem.OpenAppPackageFileAsync("Resources\\Raw\\it-IT.po");
                streams.Add("it-IT", stream);

                localizationProvider.Init(streams);

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    Application.Current.MainPage = new MainPage();
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.GetBaseException().Message);

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    Message.Text = Texts.ConnectionFailed;

                    Indicator.IsRunning = false;
                });
            }
        });
    }
}