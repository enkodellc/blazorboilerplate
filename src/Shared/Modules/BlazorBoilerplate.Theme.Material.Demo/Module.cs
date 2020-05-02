using BlazorBoilerplate.Shared.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Theme.Material.Demo
{
    public class Module : BaseModule
    {
        public override string Description => "BlazorBoilerplate demo";

        public override int Order => 2;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITagHelperComponent, ThemeTagHelperComponent>();
        }
    }
}
