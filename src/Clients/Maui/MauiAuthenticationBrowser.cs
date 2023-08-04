using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;

namespace BlazorBoilerplateMaui;

public class MauiAuthenticationBrowser : IdentityModel.OidcClient.Browser.IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
#if WINDOWS
            var result = await WinUIEx.WebAuthenticator.AuthenticateAsync(new Uri(options.StartUrl), new Uri(options.EndUrl));
#else
            var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(options.StartUrl), new Uri(options.EndUrl));
#endif

            var url = new RequestUrl(options.EndUrl)
                .Create(new Parameters(result.Properties));

            return new BrowserResult
            {
                Response = url,
                ResultType = BrowserResultType.Success
            };
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel
            };
        }
    }
}