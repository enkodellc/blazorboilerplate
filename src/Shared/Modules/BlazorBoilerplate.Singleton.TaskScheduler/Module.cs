using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Shared.ApiContentService;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Singleton.TaskScheduler
{
    public class Module : BaseModule
    {
        public override string Description => "BlazorBoilerplate.Singleton.TaskScheduler";

        public override int Order => 1001;

        private void Init(IServiceCollection services)
        {
            services.AddScoped<IApiContentProvider, ApiContentProvider>();
            services.AddHostedService<Worker>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            Init(services);
        }

    }
}
