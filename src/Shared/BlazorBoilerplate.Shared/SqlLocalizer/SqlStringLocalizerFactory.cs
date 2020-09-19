using BlazorBoilerplate.Shared.Interfaces.Db;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BlazorBoilerplate.Shared.SqlLocalizer
{
    public class SqlStringLocalizerFactory : IStringLocalizerFactory
    {
        private static IEnumerable<ILocalizationRecord> _localizationRecords = new List<ILocalizationRecord>();
        private static readonly ConcurrentDictionary<string, IStringLocalizer> _resourceLocalizations = new ConcurrentDictionary<string, IStringLocalizer>();
        private readonly IOptions<SqlLocalizationOptions> _options;
        private const string Global = "global";

        public SqlStringLocalizerFactory(IOptions<SqlLocalizationOptions> localizationOptions)
        {
            _options = localizationOptions;
        }

        public static void SetLocalizationRecords(IEnumerable<ILocalizationRecord> localizationRecords)
        {
            _localizationRecords = new List<ILocalizationRecord>(localizationRecords);
            _resourceLocalizations.Clear();
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            var returnOnlyKeyIfNotFound = _options.Value.ReturnOnlyKeyIfNotFound;
            var createNewRecordWhenLocalisedStringDoesNotExist = _options.Value.CreateNewRecordWhenLocalisedStringDoesNotExist;
            SqlStringLocalizer sqlStringLocalizer;

            if (_options.Value.UseOnlyPropertyNames)
            {
                if (_resourceLocalizations.Keys.Contains(Global))
                {
                    return _resourceLocalizations[Global];
                }

                sqlStringLocalizer = new SqlStringLocalizer(GetAllResources(Global), Global, returnOnlyKeyIfNotFound, createNewRecordWhenLocalisedStringDoesNotExist);
                return _resourceLocalizations.GetOrAdd(Global, sqlStringLocalizer);

            }
            else if (_options.Value.UseTypeFullNames)
            {
                if (_resourceLocalizations.Keys.Contains(resourceSource.FullName))
                {
                    return _resourceLocalizations[resourceSource.FullName];
                }

                sqlStringLocalizer = new SqlStringLocalizer(GetAllResources(resourceSource.FullName), resourceSource.FullName, returnOnlyKeyIfNotFound, createNewRecordWhenLocalisedStringDoesNotExist);
                return _resourceLocalizations.GetOrAdd(resourceSource.FullName, sqlStringLocalizer);
            }

            if (_resourceLocalizations.Keys.Contains(resourceSource.Name))
            {
                return _resourceLocalizations[resourceSource.Name];
            }

            sqlStringLocalizer = new SqlStringLocalizer(GetAllResources(resourceSource.Name), resourceSource.Name, returnOnlyKeyIfNotFound, createNewRecordWhenLocalisedStringDoesNotExist);
            return _resourceLocalizations.GetOrAdd(resourceSource.Name, sqlStringLocalizer);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            var returnOnlyKeyIfNotFound = _options.Value.ReturnOnlyKeyIfNotFound;
            var createNewRecordWhenLocalisedStringDoesNotExist = _options.Value.CreateNewRecordWhenLocalisedStringDoesNotExist;

            var resourceKey = baseName + location;
            if (_options.Value.UseOnlyPropertyNames)
            {
                resourceKey = Global;
            }

            if (_resourceLocalizations.Keys.Contains(resourceKey))
            {
                return _resourceLocalizations[resourceKey];
            }

            var sqlStringLocalizer = new SqlStringLocalizer(GetAllResources(resourceKey), resourceKey, returnOnlyKeyIfNotFound, createNewRecordWhenLocalisedStringDoesNotExist);
            return _resourceLocalizations.GetOrAdd(resourceKey, sqlStringLocalizer);
        }

        public void ResetCache()
        {
            ResetCache(null);
        }

        public void ResetCache(Type resourceSource = null)
        {
            if (resourceSource == null)
                _resourceLocalizations.Clear();
            else
                _resourceLocalizations.TryRemove(resourceSource.FullName, out _);
        }

        private Dictionary<string, string> GetAllResources(string resourceKey)
        {
            return _localizationRecords.Where(i => i.ResourceKey == resourceKey).ToDictionary(kvp => kvp.Key + "." + kvp.LocalizationCulture, kvp => kvp.Text);
        }
    }
}
