using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Localizer;
using Karambolo.Common;
using Karambolo.PO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace BlazorBoilerplate.Storage
{
    public class StorageLocalizationProvider : LocalizationProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly Dictionary<string, string> L;
        public StorageLocalizationProvider(ILogger<StorageLocalizationProvider> logger, IWebHostEnvironment env) : base(logger)
        {
            L = new Dictionary<string, string>();
            _environment = env;
        }
        public async Task InitDbFromPoFiles(LocalizationDbContext localizationDbContext)
        {
            Logger.LogInformation("Importing PO files in db");

            var basePath = "Localization";

            IReadOnlyDictionary<string, POCatalog> TextCatalogs = new Dictionary<string, POCatalog>();

            var textCatalogFiles = _environment.ContentRootFileProvider.GetDirectoryContents(basePath)
                .Where(fi => !fi.IsDirectory && ".po".Equals(Path.GetExtension(fi.Name)))
                .ToArray();

            var textCatalogs = new List<(string FileName, string Culture, POCatalog Catalog)>();

            var parserSettings = new POParserSettings
            {
                SkipComments = true,
                SkipInfoHeaders = true,
            };

            Parallel.ForEach(textCatalogFiles,
                () => new POParser(parserSettings),
                (file, s, p) =>
                {
                    try
                    {
                        POParseResult result;
                        using (var stream = file.CreateReadStream())
                            result = p.Parse(new StreamReader(stream));

                        if (result.Success)
                        {
                            lock (textCatalogs)
                                textCatalogs.Add((file.Name, result.Catalog.GetCultureName(), result.Catalog));
                        }
                        else
                            Logger.LogWarning($"Translation file '{file.Name}' has errors.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Translation file '{file.Name}': {ex.GetBaseException().Message}");
                    }

                    return p;
                },
                CachedDelegates.Noop<POParser>.Action);

            TextCatalogs = textCatalogs
                .GroupBy(it => it.Culture, it => (it.FileName, it.Catalog))
                .ToDictionary(g => g.Key, g => g
                    .OrderBy(it => it.FileName)
                    .Select(it => it.Catalog)
                    .Aggregate((acc, src) =>
                    {
                        foreach (var entry in src)
                            try { acc.Add(entry); }
                            catch (ArgumentException) { Logger.LogWarning($"Multiple translations for key {POStringLocalizer.FormatKey(entry.Key)}."); }

                        return acc;
                    }));

            foreach (var textCatalog in TextCatalogs.Values)
                await ImportTextCatalog(localizationDbContext, textCatalog);
        }

        public static async Task ImportTextCatalog(LocalizationDbContext localizationDbContext, POCatalog textCatalog)
        {
            if (textCatalog == null || textCatalog.Count == 0)
                throw new DomainException("File empty");

            try
            {
                var culture = new CultureInfo(textCatalog.Language.Replace("_", "-") ?? string.Empty);
            }
            catch (CultureNotFoundException)
            {
                throw new DomainException("PO File without a valid language");
            }

            var catalogCulture = textCatalog.GetCultureName();

            var pluralFormRule = await localizationDbContext.PluralFormRules
                .SingleOrDefaultAsync(i => i.Language == catalogCulture);

            if (pluralFormRule == null)
            {
                pluralFormRule = new() { Language = catalogCulture };

                localizationDbContext.PluralFormRules.Add(pluralFormRule);
            }

            pluralFormRule.Count = textCatalog.PluralFormCount;
            pluralFormRule.Selector = textCatalog.PluralFormSelector;

            await localizationDbContext.SaveChangesAsync();

            foreach (var item in textCatalog)
            {
                if (!string.IsNullOrWhiteSpace(item[0]))
                {
                    var contextId = item.Key.ContextId ?? nameof(Global);

                    var localizationRecord = await localizationDbContext.LocalizationRecords
                            .SingleOrDefaultAsync(l =>
                            l.MsgId == item.Key.Id &&
                            l.Culture == catalogCulture &&
                            l.ContextId == contextId);

                    if (localizationRecord == null)
                    {
                        localizationRecord = new()
                        {
                            Culture = catalogCulture,
                            MsgId = item.Key.Id,
                            ContextId = contextId
                        };

                        localizationDbContext.LocalizationRecords.Add(localizationRecord);
                    }

                    localizationRecord.MsgIdPlural = item.Key.PluralId;
                    localizationRecord.Translation = item[0];

                    await localizationDbContext.SaveChangesAsync();

                    if (item.Count == textCatalog.PluralFormCount)
                    {
                        var i = 0;

                        foreach (var entry in item)
                            if (!string.IsNullOrWhiteSpace(entry))
                            {
                                var pluralTranslation = await localizationDbContext.PluralTranslations
                                    .SingleOrDefaultAsync(l =>
                                    l.LocalizationRecordId == localizationRecord.Id && l.Index == i);

                                if (pluralTranslation == null)
                                {
                                    pluralTranslation = new()
                                    {
                                        LocalizationRecordId = localizationRecord.Id,
                                        Index = i
                                    };

                                    localizationDbContext.PluralTranslations.Add(pluralTranslation);
                                }

                                pluralTranslation.Translation = entry;

                                i++;
                            }

                        await localizationDbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
