using BlazorBoilerplate.Shared.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Shared.Models
{
    public abstract class BaseModule : IModule
    {
        public virtual string Name => GetType().Namespace;

        public virtual string Description => GetType().Namespace;

        public virtual int Order => 1;

        public virtual void ConfigureServices(IServiceCollection services)
        { }

        public virtual void ConfigureWebAssemblyHost(WebAssemblyHost webAssemblyHost)
        { }

        public virtual void ConfigureWebAssemblyServices(IServiceCollection services)
        { }
    }
}
