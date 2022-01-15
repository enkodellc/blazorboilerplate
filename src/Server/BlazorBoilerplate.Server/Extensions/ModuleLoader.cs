using BlazorBoilerplate.Shared.Providers;
using System.Reflection;
using System.Runtime.Loader;

namespace BlazorBoilerplate.Server.Extensions
{
    public static class ModuleLoader
    {
        static ModuleLoader()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);

            var assemblyThemePath = Path.Combine(@$"{assemblyPath}", "Themes/MudBlazor");

            List<Assembly> allAssemblies = new();

            if (Directory.Exists(assemblyThemePath))
            {
                foreach (var dll in Directory.GetFiles(assemblyThemePath, "*.dll"))
                    allAssemblies.Add(AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll))));
            }

            var assemblyModulesPath = Path.Combine(@$"{assemblyPath}", "Modules");

            if (Directory.Exists(assemblyModulesPath))
            {
                foreach (var dll in Directory.GetFiles(assemblyModulesPath, "*.dll"))
                    allAssemblies.Add(AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(File.ReadAllBytes(dll))));
            }

            ModuleProvider.Init(allAssemblies);
        }

        public static IServiceCollection AddModules(this IServiceCollection services)
        {
            if (ModuleProvider.Modules != null)
                foreach (var moduleInstance in ModuleProvider.Modules)
                    moduleInstance.ConfigureServices(services);

            return services;
        }
    }
}
