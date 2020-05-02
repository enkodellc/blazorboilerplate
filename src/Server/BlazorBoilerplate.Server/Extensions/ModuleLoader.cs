using BlazorBoilerplate.Shared.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace BlazorBoilerplate.Server.Extensions
{
    public static class ModuleLoader
    {      
        static ModuleLoader()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            assemblyPath = @$"{assemblyPath}\Modules";

            List<Assembly> allAssemblies = new List<Assembly>();

            foreach (var dll in Directory.GetFiles(assemblyPath, "*.dll"))
                allAssemblies.Add(AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll))));

            ModuleProvider.Init(allAssemblies);          
        }

        public static IServiceCollection AddModules(this IServiceCollection services)
        {
            foreach (var moduleInstance in ModuleProvider.Modules)
            {
                moduleInstance.ConfigureServices(services);
            }

            return services;
        }
    }
}
