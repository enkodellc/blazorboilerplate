using System;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Client
{
    public class DryIocServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        public IContainer CreateBuilder(IServiceCollection services)
        {
            var container = new Container().WithDependencyInjectionAdapter(services);
            container.RegisterDescriptor(ServiceDescriptor.Singleton(typeof(IContainer), container));

            return container;
        }

        public IServiceProvider CreateServiceProvider(IContainer containerBuilder)
            => containerBuilder.ConfigureServiceProvider<CompositionRoot>();
    }

    internal class CompositionRoot
    {
        public CompositionRoot() { }
    }
}
