using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace BlazorBoilerplate.Shared.Localizer
{
    public static class LocalizationServiceCollectionExtensions
    {
        public static IServiceCollection AddTextLocalization(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();

            AddTextLocalizationServices(services);

            return services;
        }

        public static IServiceCollection AddTextLocalization(
            this IServiceCollection services,
            Action<TextLocalizationOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            AddTextLocalizationServices(services, setupAction);

            return services;
        }

        // To enable unit testing
        internal static void AddTextLocalizationServices(IServiceCollection services)
        {
            services.TryAddSingleton<IStringLocalizerFactory, TextLocalizerFactory>();
            services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        }

        internal static void AddTextLocalizationServices(
            IServiceCollection services,
            Action<TextLocalizationOptions> setupAction)
        {
            AddTextLocalizationServices(services);
            services.Configure(setupAction);
        }
    }
}
