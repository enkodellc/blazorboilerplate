using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using BlazorBoilerplate.Shared.Localizer;
using Karambolo.Common;
using Karambolo.PO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            var cultures = _environment.ContentRootFileProvider.GetDirectoryContents(basePath)
                .Where(fi => fi.IsDirectory)
                .Select(fi => fi.Name)
                .ToArray();

            var textCatalogFiles = cultures.SelectMany(
                c => _environment.ContentRootFileProvider.GetDirectoryContents(Path.Combine(basePath, c))
                .Where(fi => !fi.IsDirectory && ".po".Equals(Path.GetExtension(fi.Name), StringComparison.OrdinalIgnoreCase)),
                (c, f) => (Culture: c, FileInfo: f));

            var textCatalogs = new List<(string FileName, string Culture, POCatalog Catalog)>();

            var parserSettings = new POParserSettings
            {
                SkipComments = true,
                SkipInfoHeaders = true,
            };

            Parallel.ForEach(textCatalogFiles,
                () => new POParser(parserSettings),
                (it, s, p) =>
                {
                    POParseResult result;
                    using (var stream = it.FileInfo.CreateReadStream())
                        result = p.Parse(new StreamReader(stream));

                    if (result.Success)
                    {
                        if (result.Catalog.GetCultureName() != it.Culture)
                            Logger.LogWarning($"Translation file '{Path.Combine(basePath, it.Culture, it.FileInfo.Name)}' language / folder mismatch.");
                        else
                        {
                            lock (textCatalogs)
                                textCatalogs.Add((it.FileInfo.Name, it.Culture, result.Catalog));
                        }
                    }
                    else
                        Logger.LogWarning($"Translation file '{Path.Combine(basePath, it.Culture, it.FileInfo.Name)}' has errors.");

                    return p;
                },
                Noop<POParser>.Action);

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

        public async Task ImportTextCatalog(LocalizationDbContext localizationDbContext, POCatalog textCatalog)
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

            await localizationDbContext.Upsert(new PluralFormRule()
            {
                Language = textCatalog.GetCultureName(),
                Count = textCatalog.PluralFormCount,
                Selector = textCatalog.PluralFormSelector
            }).RunAsync();

            foreach (var item in textCatalog)
            {
                if (!string.IsNullOrWhiteSpace(item.First()))
                {
                    var localizationRecord = new LocalizationRecord()
                    {
                        Culture = textCatalog.GetCultureName(),
                        MsgId = item.Key.Id,
                        MsgIdPlural = item.Key.PluralId,
                        Translation = item.First(),
                        ContextId = item.Key.ContextId ?? nameof(Global)
                    };

                    await localizationDbContext.Upsert(localizationRecord).On(l => new { l.MsgId, l.Culture, l.ContextId }).RunAsync();

                    if (item.Count == textCatalog.PluralFormCount)
                    {
                        localizationRecord = await localizationDbContext.LocalizationRecords
                            .SingleAsync(l =>
                            localizationRecord.MsgId == l.MsgId &&
                            localizationRecord.Culture == l.Culture &&
                            localizationRecord.ContextId == l.ContextId);

                        var pluralTranslations = new List<PluralTranslation>();

                        var i = 0;

                        foreach (var entry in item)
                            if (!string.IsNullOrWhiteSpace(entry))
                                pluralTranslations.Add(new PluralTranslation()
                                {
                                    LocalizationRecordId = localizationRecord.Id,
                                    Index = i++,
                                    Translation = entry
                                });

                        await localizationDbContext.PluralTranslations.UpsertRange(pluralTranslations).On(l => new { l.LocalizationRecordId, l.Index }).RunAsync();
                    }
                }
            }
        }
    }
}
