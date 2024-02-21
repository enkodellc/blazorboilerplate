using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Shared.Models;
using BlazorBoilerplate.Theme.Material.Demo.Shared.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Theme.Material.Demo
{
    public class Module : BaseModule
    {
        public override string Description => "BlazorBoilerplate demo";

        public override int Order => 2;

        private void Init(IServiceCollection services)
        {
            services.AddSingleton<IDynamicComponent, NavMenu>();
            services.AddSingleton<IDynamicComponent, Footer>();
            services.AddSingleton<IDynamicComponent, DrawerFooter>();
            services.AddSingleton<IDynamicComponent, TopRightBarSection>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            Init(services);
        }

        public override void ConfigureWebAssemblyServices(IServiceCollection services)
        {
            Init(services);
        }
    }
}
