using BlazorBoilerplate.Shared.DataInterfaces;
using Karambolo.PO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    foreach (var culture in localizationRecords.Select(i => i.Culture).Distinct())
                    {
                        var catalog = new POCatalog();

                        catalog.Encoding = "UTF-8";

                        var pluralFormRule = pluralFormRules.Single(i => i.Language == culture);

                        catalog.PluralFormCount = pluralFormRule.Count;
                        catalog.PluralFormSelector = pluralFormRule.Selector;
                        catalog.Language = culture;

                        catalog.Headers = new Dictionary<string, string>
                        {
                            { "X-Generator", "BlazorBoilerplate" },
                        };

                        foreach (var localizationRecord in localizationRecords.Where(i => i.Culture == culture))
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
                    Logger.LogError($"CreateCatalogsFromDb: {ex.GetBaseException().Message}");
                }

                TextCatalogs = textCatalogs;
            }
        }
    }
}
