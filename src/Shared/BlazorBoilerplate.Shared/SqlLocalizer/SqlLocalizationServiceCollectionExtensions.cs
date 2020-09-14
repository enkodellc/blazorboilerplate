using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using System;

namespace BlazorBoilerplate.Shared.SqlLocalizer
{
    public static class SqlLocalizationServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlLocalization(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();

            AddSqlLocalizationServices(services);

            return services;
        }

        public static IServiceCollection AddSqlLocalization(
            this IServiceCollection services,
            Action<SqlLocalizationOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            AddSqlLocalizationServices(services, setupAction);

            return services;
        }

        // To enable unit testing
        internal static void AddSqlLocalizationServices(IServiceCollection services)
        {
            services.TryAddSingleton<IStringLocalizerFactory, SqlStringLocalizerFactory>();
            services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        }

        internal static void AddSqlLocalizationServices(
            IServiceCollection services,
            Action<SqlLocalizationOptions> setupAction)
        {
            AddSqlLocalizationServices(services);
            services.Configure(setupAction);
        }
    }
}

