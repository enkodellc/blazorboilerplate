using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace BlazorBoilerplate.Shared.TagHelpers
{
    [HtmlTargetElement("app")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AppTagHelperComponentTagHelper : TagHelperComponentTagHelper
    {
        public AppTagHelperComponentTagHelper(
            ITagHelperComponentManager componentManager,
            ILoggerFactory loggerFactory) : base(componentManager, loggerFactory)
        {
        }
    }
}
