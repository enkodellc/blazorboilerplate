using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplate.GoogleAnalytics
{
    public class GoogleAnalyticsTagHelperComponent : TagHelperComponent
    {
        private readonly string trackingId;

        public override int Order => 1000;

        public GoogleAnalyticsTagHelperComponent(ILogger<GoogleAnalyticsTagHelperComponent> logger, IConfiguration configuration)
        {
            trackingId = configuration["Modules:BlazorBoilerplate.GoogleAnalytics:TrackingId"];

            if (trackingId == null)
                logger.LogError("Unable to find BlazorBoilerplate.GoogleAnalytics:TrackingId in Modules section of Configuration.");
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (trackingId != null && string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml(@$"
<!-- Global site tag (gtag.js) - Google Analytics -->
<script async src=""https://www.googletagmanager.com/gtag/js?id={trackingId}""></script>
<script>
    window.dataLayer = window.dataLayer || [];
    function gtag() {{ dataLayer.push(arguments); }}
    gtag('js', new Date());
    gtag('config', '{trackingId}');
</script>");
            }

            return Task.CompletedTask;
        }
    }
}
