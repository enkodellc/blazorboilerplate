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
        public static IEnumerable<IModule> Modules { get; private set; }
        public static RootComponentMapping RootComponentMapping { get; private set; }
        public static IEnumerable<Assembly> AssembliesWithPages { get; private set; }

        public static void Init(IEnumerable<Assembly> assemblies)
        {
            AssembliesWithPages = assemblies.Where(w => w.ExportedTypes.Any(t => t.GetCustomAttributes<RouteAttribute>(inherit: false).Any())).ToList();

            //TODO a table should define the mapping between tenant and module
            //only one ITheme module per tenant. Now I simply take the first one.
            var theme = AssembliesWithPages.First(w => w.ExportedTypes.Any(t => t.GetInterfaces().Contains(typeof(ITheme))));

            ((List<Assembly>)AssembliesWithPages).Remove(theme);

            ITheme themeInstance = (ITheme)Activator.CreateInstance(theme.ExportedTypes.Single(t => t.GetInterfaces().Contains(typeof(ITheme))));

            RootComponentMapping = themeInstance.RootComponentMapping;

            var modules = new List<IModule>();

            foreach (var assembly in assemblies)
            {
                var implementationType = assembly.ExportedTypes.SingleOrDefault(t => !t.IsAbstract && t.GetInterfaces().Contains(typeof(IModule)));

                if (implementationType != null)
                {
                    IModule moduleInstance = (IModule)Activator.CreateInstance(implementationType);

                    modules.Add(moduleInstance);
                }
            }

            Modules = modules.OrderBy(m => m.Order);
        }
    }
}
