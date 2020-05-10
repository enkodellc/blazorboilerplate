using System;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IModule
    {
        string Name { get; }

        string Description { get; }

        int Order { get; }

        void Configure(IServiceCollection services);

        void ConfigureWebAssembly(IServiceCollection services);

        void InitializeWebAssemblyHost(WebAssemblyHost webAssemblyHost);
    }
}
