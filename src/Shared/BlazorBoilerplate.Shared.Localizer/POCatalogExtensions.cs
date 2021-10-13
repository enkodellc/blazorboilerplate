using Karambolo.PO;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BlazorBoilerplate.Shared.Localizer
{
    public static class POCatalogExtensions
    {
        public static string GetCultureName(this POCatalog catalogo) => new CultureInfo(catalogo.Language.Replace("_", "-")).Name;

        public static MarkupString ToMarkup(this LocalizedString localizedString) => (MarkupString)localizedString.Value;
    }
}
