using BlazorBoilerplate.Shared.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.GoogleAnalytics
{
    public class Module : BaseModule
    {
        public override string Name => "Google Analytics";

        public override string Description => ModuleStrings.Description;

        public override int Order => 1000;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITagHelperComponent, GoogleAnalyticsTagHelperComponent>();
        }
    }
}
