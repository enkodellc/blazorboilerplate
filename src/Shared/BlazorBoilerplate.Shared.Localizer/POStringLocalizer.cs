using Karambolo.Common.Localization;
using Karambolo.PO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace BlazorBoilerplate.Shared.Localizer
{
    public abstract class POStringLocalizerBase : NullStringLocalizer
    {
        public ILogger Logger { get; set; }

        protected POStringLocalizerBase(string contextId, IOptions<TextLocalizationOptions> localizationOptions) : base(contextId, localizationOptions)
        {
            Logger = NullLogger.Instance;
        }

        protected abstract POCatalog Catalog { get; }

        protected abstract POCatalog ParentCultureCatalog { get; }

        protected abstract POCatalog FallBackCatalog { get; }

        bool TryGetTranslationCore(POKey key, int pluralCount, out string value, POCatalog catalog = null)
        {
            if (catalog == null)
                catalog = Catalog;

            if (catalog != null)
            {
                var translation = catalog.GetTranslation(key, pluralCount);
                if (translation != null)
                {
                    value = translation;
                    return true;
                }
                else
                {
                    translation = ParentCultureCatalog?.GetTranslation(key, pluralCount);
                    if (translation != null)
                    {
                        value = translation;
                        return true;
                    }
                    else if (Options.Value.FallBackNeutralCulture && catalog.GetCultureName() != Settings.NeutralCulture)
                    {
                        return TryGetTranslationCore(key, pluralCount, out value, FallBackCatalog);
                    }
                }
            }

            value = null;

            return false;
        }

        protected override bool TryGetTranslation(string name, Plural plural, TextContext context, out string value)
        {
            var key = new POKey(name, plural.Id, context.Id);
            if (!TryGetTranslationCore(key, plural.Count, out string translation))
            {
                Logger.LogTrace("No translation for key {0}.", POStringLocalizer.FormatKey(key));

                base.TryGetTranslation(name, plural, context, out value);
                return false;
            }

            value = translation;
            return true;
        }
    }

    public class POStringLocalizer : POStringLocalizerBase
    {
        public static string FormatKey(POKey key)
        {
            var result = string.Concat("'", key.Id, "'");
            if (key.PluralId != null)
                result = string.Concat(result, "-'", key.PluralId, "'");
            if (key.ContextId != null)
                result = string.Concat(result, "@'", key.ContextId, "'");

            return result;
        }

        readonly POCatalog _catalog;
        readonly POCatalog _parentCultureCatalog;
        readonly POCatalog _fallBackCatalog;
        readonly Func<CultureInfo, bool, POCatalog> _getCatalogForCulture;

        public POStringLocalizer(CultureInfo culture, string contextId, Func<CultureInfo, bool, POCatalog> getCatalogForCulture, IOptions<TextLocalizationOptions> localizationOptions) : base(contextId, localizationOptions)
        {
            _catalog = getCatalogForCulture(culture, false);
            _parentCultureCatalog = getCatalogForCulture(culture.Parent, true);
            _fallBackCatalog = getCatalogForCulture(new CultureInfo(Settings.NeutralCulture), false);
            _getCatalogForCulture = getCatalogForCulture;
        }

        protected override POCatalog Catalog => _catalog;

        protected override POCatalog ParentCultureCatalog => _parentCultureCatalog;

        protected override POCatalog FallBackCatalog => _fallBackCatalog;

        public override IExtendedStringLocalizer WithCulture(CultureInfo culture)
        {
            return new POStringLocalizer(culture, ContextId, _getCatalogForCulture, Options) { Logger = Logger };
        }
    }
}
