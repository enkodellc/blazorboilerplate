using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace BlazorBoilerplate.Shared.Localizer
{
    public class TextLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILocalizationProvider _provider;
        private readonly IOptions<TextLocalizationOptions> _options;
        public TextLocalizerFactory(ILocalizationProvider provider, IOptions<TextLocalizationOptions> localizationOptions)
        {
            _provider = provider;
            _options = localizationOptions;
        }

        IStringLocalizer IStringLocalizerFactory.Create(Type resourceSource)
        {
            var contextId = resourceSource.FullName;

            if (resourceSource == typeof(Global) || !_options.Value.UseTypeFullNames)
                contextId = resourceSource.Name;

            return new LocalizationManager(_provider, contextId, _options);
        }

        IStringLocalizer IStringLocalizerFactory.Create(string baseName, string location)
        {
            return new LocalizationManager(_provider, baseName + location, _options);
        }
    }
}
