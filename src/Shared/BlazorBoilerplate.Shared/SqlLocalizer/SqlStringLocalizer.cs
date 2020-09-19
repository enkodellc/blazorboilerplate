using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BlazorBoilerplate.Shared.SqlLocalizer
{
    public class SqlStringLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, string> _localizations;

        private readonly string _resourceKey;
        private bool _returnKeyOnlyIfNotFound;
        private bool _createNewRecordWhenLocalisedStringDoesNotExist;

        public SqlStringLocalizer(Dictionary<string, string> localizations, string resourceKey, bool returnKeyOnlyIfNotFound, bool createNewRecordWhenLocalisedStringDoesNotExist)
        {
            _localizations = localizations;
            _resourceKey = resourceKey;
            _returnKeyOnlyIfNotFound = returnKeyOnlyIfNotFound;
            _createNewRecordWhenLocalisedStringDoesNotExist = createNewRecordWhenLocalisedStringDoesNotExist;
        }
        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var text = GetText(name, out bool notSucceed);

                return new LocalizedString(name, text, notSucceed);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var text = GetText(name, out bool notSucceed);

                return new LocalizedString(name, string.Format(text, arguments), notSucceed);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();

        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetText(string key, out bool notSucceed)
        {
            var culture = CultureInfo.CurrentCulture.ToString();

            string computedKey = $"{key}.{culture}";

            string result;

            if (_localizations.TryGetValue(computedKey, out result))
            {
                notSucceed = false;
                return result;
            }
            else
            {
                culture = CultureInfo.CurrentCulture.Parent.ToString();
                computedKey = $"{key}.{culture}";

                if (_localizations.TryGetValue(computedKey, out result))
                {
                    notSucceed = false;
                    return result;
                }
                else
                {
                    culture = "en";
                    computedKey = _localizations.Keys.FirstOrDefault(k => k.StartsWith($"{key}.{culture}"));

                    if (computedKey != null && _localizations.TryGetValue(computedKey, out result))
                    {
                        notSucceed = false;
                        return result;
                    }
                    else
                    {
                        notSucceed = true;

                        if (_returnKeyOnlyIfNotFound)
                        {
                            return key;
                        }

                        return $"{_resourceKey}.{key}.{culture}";
                    }
                }
            }
        }
    }
}
