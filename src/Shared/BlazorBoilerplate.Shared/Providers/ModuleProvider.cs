using BlazorBoilerplate.Shared.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlazorBoilerplate.Shared.Providers
{
    public static class ModuleProvider
    {
        public static IEnumerable<IModule> Modules { get; set; }
        public static RootComponentMapping RootComponentMapping { get; set; }
        public static IEnumerable<Assembly> AssembliesWithPages { get; set; }
    }
}
