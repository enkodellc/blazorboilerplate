using Microsoft.AspNetCore.ProtectedBrowserStorage;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering Protected Browser Storage services.
    /// </summary>
    public static class ProtectedBrowserStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for protected browser storage to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public static void AddProtectedBrowserStorage(this IServiceCollection services)
        {
            services.AddScoped<ProtectedLocalStorage>();
            services.AddScoped<ProtectedSessionStorage>();
        }
    }
}
