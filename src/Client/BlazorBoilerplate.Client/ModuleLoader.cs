using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorLazyLoading.Abstractions;
using DryIoc;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DryIoc.Microsoft.DependencyInjection;

namespace BlazorBoilerplate.Extensions
{
    public static class ModuleLoader
    {
        public readonly static List<RootComponentMapping> RootComponents = new List<RootComponentMapping>();

        static List<IModule> modules = new List<IModule>();
        static List<ITheme> themes = new List<ITheme>();

        public static async Task LoadModules(this IServiceProvider services)
        {
            var manifests = await services.GetRequiredService<IManifestRepository>().GetAllAsync().ConfigureAwait(false);
            var moduleNames = manifests.Select(m => m.ModuleName);

            Console.WriteLine($"Found modules: {string.Join(';', moduleNames)}");

            // Load Assemblies
            foreach (var moduleName in moduleNames)
            {
                Assembly asm = await LoadAssembly(moduleName, services.GetRequiredService<IAssemblyLoader>());
                if (asm == null) continue;

                var module = CreateTypeFromAssembly<IModule>(asm, services);

                if (module != null)
                {
                    modules.Add(module);
                }

                var theme = CreateTypeFromAssembly<ITheme>(asm, services);

                if (theme != null)
                {
                    themes.Add(theme);
                }
            }

            modules = modules.OrderBy(m => m.Order).ToList();
            RootComponents.AddRange(themes.Select(t => t.RootComponentMapping));
        }

        public static void ConfigureModules(this WebAssemblyHostBuilder hostBuilder)
        {
            hostBuilder.RootComponents.AddRange(RootComponents);
        }

        public static void InitializeModules(this IServiceProvider provider, WebAssemblyHost host)
        {
            var container = provider.GetRequiredService<IContainer>();

            foreach (var module in modules)
            {
                // configure new services
                var services = new ServiceCollection();

                if (host == null)
                {
                    module.Configure(services);
                }
                else
                {
                    module.ConfigureWebAssembly(services);
                }

                // add new services to the global container
                foreach (var descriptor in services)
                {
                    container.RegisterDescriptor(descriptor);
                }

                // initialize webhost after registering the services
                if (host != null)
                {
                    module.InitializeWebAssemblyHost(host);
                }
            }
        }

        private static Task<Assembly> LoadAssembly(string moduleName, IAssemblyLoader loader)
        {
            return loader.LoadAssemblyByNameAsync(new AssemblyName
            {
                Name = moduleName,
                Version = null, // we dont care about the module version
            });
        }

        private static T CreateTypeFromAssembly<T>(Assembly assembly, IServiceProvider services)
            where T : class
        {
            var moduleType = assembly.GetTypes().FirstOrDefault(t => typeof(T).IsAssignableFrom(t));

            if (moduleType == null)
            {
                return null;
            }

            return (T)ActivatorUtilities.CreateInstance(services, moduleType);
        }
    }
}
