using System.Globalization;

namespace BlazorBoilerplate.Shared.Localizer
{
    public class Global
    {
        private static CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        public static string GetCountryName(string countryCode)
        {
            return countryCode != null ? new RegionInfo(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName + "-" + countryCode).NativeName.Replace("Itàlia", "Italia") : null;
        }

        public static string GetLanguageName(string languageCode)
        {
            return allCultures.FirstOrDefault(i => i.Name == languageCode).DisplayName;
        }
    }
}
