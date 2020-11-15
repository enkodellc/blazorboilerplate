﻿using Karambolo.PO;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;

namespace BlazorBoilerplate.Shared.Localizer
{
    public interface ILocalizationManager : IExtendedStringLocalizer
    {
        ILocalizationProvider Provider { get; }
        CultureInfo CurrentCulture { get; set; }
    }

    public class LocalizationManager : POStringLocalizerBase, ILocalizationManager
    {
        public LocalizationManager(ILocalizationProvider provider, string contextId, IOptions<TextLocalizationOptions> localizationOptions) : base(contextId, localizationOptions)
        {
            Provider = provider;
        }

        public ILocalizationProvider Provider { get; }

        protected override POCatalog Catalog => GetCatalogForCulture(CurrentCulture);

        protected override POCatalog FallBackCatalog => GetCatalogForCulture(new CultureInfo(Settings.NeutralCulture));

        public CultureInfo CurrentCulture
        {
            get => CultureInfo.CurrentUICulture;
            set => throw new NotSupportedException();
        }

        POCatalog GetCatalogForCulture(CultureInfo culture)
        {
            while (culture != null && !culture.Equals(CultureInfo.InvariantCulture))
                if (Provider.TextCatalogs.TryGetValue(culture.Name, out var catalog))
                    return catalog;
                else
                    culture = culture.Parent;

            return null;
        }

        public override IExtendedStringLocalizer WithCulture(CultureInfo culture)
        {
            return new POStringLocalizer(culture, ContextId, GetCatalogForCulture, Options) { Logger = Logger };
        }
    }
}
