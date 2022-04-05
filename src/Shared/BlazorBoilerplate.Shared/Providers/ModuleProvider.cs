using BlazorBoilerplate.Shared.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

namespace BlazorBoilerplate.Shared.Providers
{
    public static class ModuleProvider
    {
        private static List<Assembly> assembliesWithPages;
        private static List<IModule> modules;

        public static IEnumerable<IModule> Modules { get => modules; }
        public static RootComponentMapping RootComponentMapping { get; private set; }
        public static IEnumerable<Assembly> AssembliesWithPages { get => assembliesWithPages; }

        public static void Init(IEnumerable<Assembly> assemblies)
        {
            assembliesWithPages = assemblies.Where(w => w.ExportedTypes.Any(t => t.GetCustomAttributes<RouteAttribute>(inherit: false).Any())).ToList();

            //TODO a table should define the mapping between tenant and module
            //only one ITheme module per tenant. Now I simply take the first one.
            var theme = assembliesWithPages.FirstOrDefault(w => w.ExportedTypes.Any(t => t.GetInterfaces().Contains(typeof(ITheme))));

            if (theme == null)
                throw new Exception("No module contains ITheme");
            else
            {
                ((List<Assembly>)assembliesWithPages).Remove(theme);

                ITheme themeInstance = (ITheme)Activator.CreateInstance(theme.ExportedTypes.Single(t => t.GetInterfaces().Contains(typeof(ITheme))));

                RootComponentMapping = themeInstance.RootComponentMapping;

                modules = new List<IModule>();

                foreach (var assembly in assemblies)
                {
                    var implementationType = assembly.ExportedTypes.SingleOrDefault(t => !t.IsAbstract && t.GetInterfaces().Contains(typeof(IModule)));

                    if (implementationType != null)
                    {
                        IModule moduleInstance = (IModule)Activator.CreateInstance(implementationType);

                        modules.Add(moduleInstance);
                    }
                }

                modules = modules.OrderBy(m => m.Order).ToList();
            }
        }

        //TODO init modules
        public static void AddModules(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                if (!assembliesWithPages.Contains(assembly) && assembly.ExportedTypes.Any(t => t.GetCustomAttributes<RouteAttribute>(inherit: false).Any()))
                    assembliesWithPages.Add(assembly);
            }
        }
    }
}
