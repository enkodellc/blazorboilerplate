using BlazorBoilerplate.Shared.DataInterfaces;
using Karambolo.PO;
using Microsoft.Extensions.Logging;

namespace BlazorBoilerplate.Shared.Localizer
{
    public class LocalizationProvider : ILocalizationProvider
    {
        protected readonly ILogger Logger;
        public LocalizationProvider(ILogger<LocalizationProvider> logger)
        {
            Logger = logger;
        }

        public string[] Cultures { get; private set; }

        public IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; private set; }

        public void Init(IEnumerable<ILocalizationRecord> localizationRecords, IEnumerable<IPluralFormRule> pluralFormRules, bool reLoad = false)
        {
            if (TextCatalogs == null || TextCatalogs.Count == 0 || reLoad)
            {
                var textCatalogs = new Dictionary<string, POCatalog>();

                try
                {

                    foreach (var culture in localizationRecords.Select(i => i.Culture).Distinct().ToList())
                    {
                        var pluralFormRule = pluralFormRules.SingleOrDefault(i => i.Language == culture);

                        if (pluralFormRule == null)
                        {
                            Logger.LogError($"Missing PluralFormRule for {culture}");
                            continue;
                        }

                        var catalog = new POCatalog
                        {
                            Encoding = "UTF-8",
                            PluralFormCount = pluralFormRule.Count,
                            PluralFormSelector = pluralFormRule.Selector,
                            Language = culture,
                            Headers = new Dictionary<string, string>
                        {
                            { "X-Generator", "BlazorBoilerplate" },
                        }
                        };

                        foreach (var localizationRecord in localizationRecords.Where(i => i.Culture == culture).ToList())
                        {
                            try
                            {
                                if (localizationRecord.PluralTranslations.Count == 0)
                                {
                                    catalog.Add(new POSingularEntry(new POKey(localizationRecord.MsgId, contextId: localizationRecord.ContextId))
                                    {
                                        Translation = localizationRecord.Translation
                                    });
                                }
                                else
                                {
                                    var entry = new POPluralEntry(new POKey(localizationRecord.MsgId, localizationRecord.MsgIdPlural, localizationRecord.ContextId),
                                        localizationRecord.PluralTranslations.OrderBy(i => i.Index).Select(i => i.Translation));

                                    catalog.Add(entry);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError($"Import localizationRecord {localizationRecord.MsgId}: {ex.GetBaseException().Message}");
                            }
                        }

                        textCatalogs.Add(culture, catalog);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Init: {ex.GetBaseException().StackTrace}");
                }

                TextCatalogs = textCatalogs;
            }
        }
    }
}
