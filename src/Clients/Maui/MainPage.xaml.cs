using Microsoft.AspNetCore.Components.WebView;

namespace BlazorBoilerplateMaui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

        blazorWebView.UrlLoading +=
            (sender, urlLoadingEventArgs) =>
            {
                if (urlLoadingEventArgs.Url.Host != "0.0.0.0")
                {
                    urlLoadingEventArgs.UrlLoadingStrategy = UrlLoadingStrategy.OpenExternally;
                }
            };
    }
}
