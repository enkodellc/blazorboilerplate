using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Localizer;
using BlazorBoilerplateMaui.Resources;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplateMaui;

public partial class LoadingPage : ContentPage
{
	public LoadingPage(
        ILocalizationApiClient localizationApiClient,
        ILocalizationProvider localizationProvider,
        ILogger<App> logger)
	{
		InitializeComponent();

        Task.Run(async () =>
        {
            try
            {
                var localizationRecordsTask = localizationApiClient.GetLocalizationRecords();

                var pluralFormRulesTask = localizationApiClient.GetPluralFormRules();
                await Task.WhenAll(new Task[] { localizationRecordsTask, pluralFormRulesTask });

                localizationProvider.Init(localizationRecordsTask.Result, pluralFormRulesTask.Result);

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