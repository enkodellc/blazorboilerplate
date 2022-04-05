using Karambolo.Common;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace BlazorBoilerplate.Shared.Localizer
{
    public class NullStringLocalizer : IExtendedStringLocalizer
    {
        protected readonly string ContextId;

        protected readonly IOptions<TextLocalizationOptions> Options;
        protected NullStringLocalizer(string contextId, IOptions<TextLocalizationOptions> localizationOptions)
        {
            ContextId = contextId;
            Options = localizationOptions;
        }

        protected virtual bool TryGetTranslation(string name, Plural plural, TextContext context, out string value)
        {
            value = plural.Id == null || plural.Count == 1 ? name : plural.Id;

            if (!Options.Value.ReturnOnlyKeyIfNotFound)
                value = $"{context.Id}.{value}";

            return true;
        }

        public bool TryGetTranslation(string name, object[] arguments, out string value)
        {
            var plural = default(Plural);
            var context = TextContext.From(ContextId);

            if (!ArrayUtils.IsNullOrEmpty(arguments))
            {
                var pluralIndex = Array.FindIndex(arguments, a => a is Plural);
                if (pluralIndex >= 0)
                    plural = (Plural)arguments[pluralIndex];

                var contextIndex = arguments.Length - 1;
                object contextArg;
                if (pluralIndex != contextIndex && (contextArg = arguments[contextIndex]) is TextContext)
                    context = (TextContext)contextArg;
            }

            return TryGetTranslation(name, plural, context, out value);
        }

        LocalizedString Localize(string name, object[] arguments)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            bool notFound;
            if (notFound = !TryGetTranslation(name, arguments, out string translation) && translation == null)
                translation = name;

            var value = ArrayUtils.IsNullOrEmpty(arguments) ? translation : string.Format(translation, arguments);
            return new LocalizedString(name, value, notFound);
        }

        public LocalizedString this[string name] => Localize(name, null);

        public LocalizedString this[string name, params object[] arguments] => Localize(name, arguments);

        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotSupportedException();
        }

        public virtual IExtendedStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }
    }
}
