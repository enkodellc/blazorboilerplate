﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorBoilerplate.Shared.Interfaces
{
    public interface IModule
    {
        string Name { get; }

        string Description { get; }

        int Order { get; }

        void ConfigureServices(IServiceCollection services);

        void ConfigureWebAssemblyServices(IServiceCollection services);

        void ConfigureWebAssemblyHost(WebAssemblyHost webAssemblyHost);
    }
}
