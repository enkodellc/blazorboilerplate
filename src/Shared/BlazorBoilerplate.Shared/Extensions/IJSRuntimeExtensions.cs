using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorBoilerplate.Shared.Extensions
{
    public static class IJSRuntimeExtensions
    {
        private static Regex regExAspNetCoreCulture = new Regex(@"c=(?<culture>[A-Za-z]{1,8}(-[A-Za-z0-9]{1,8}))\|uic=(?<uiculture>[A-Za-z]{1,8}(-[A-Za-z0-9]{1,8}))");
        public static async Task<string> GetAspNetCoreCultureCookie(this IJSRuntime js)
        {
            var culture = await js.InvokeAsync<string>("cookieStorage.get", ".AspNetCore.Culture");

            if (culture != null)
            {
                if (regExAspNetCoreCulture.IsMatch(culture))
                    culture = regExAspNetCoreCulture.Match(culture).Groups["uiculture"].Value;
                else
                    culture = null;
            }

            return culture;
        }
        public static async Task SetAspNetCoreCultureCookie(this IJSRuntime js, string culture)
        {
            var escapedCulture = Uri.EscapeDataString($"c={culture}|uic={culture}");
            await js.InvokeVoidAsync("cookieStorage.set", $".AspNetCore.Culture={escapedCulture}; max-age=2592000;path=/");
        }
    }
}
