﻿using BlazorBoilerplate.Shared.DataInterfaces;
using Karambolo.PO;
using System.Collections.Generic;

namespace BlazorBoilerplate.Shared.Localizer
{
    public interface ILocalizationProvider
    {
        string[] Cultures { get; }
        IReadOnlyDictionary<string, POCatalog> TextCatalogs { get; }
        void Init(IEnumerable<ILocalizationRecord> localizationRecords, IEnumerable<IPluralFormRule> pluralFormRules, bool reLoad = false);
    }
}
