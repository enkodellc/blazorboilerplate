namespace BlazorBoilerplate.Localization
{
    public static class Settings
    {
        public static readonly string[] SupportedCultures = { "en-US", "de-DE", "it-IT", "fa-IR" };

        public static readonly (string, string)[] SupportedCulturesWithName = new[] { ("English", "en-US"),("Deutsch", "de-DE"), ("Italiano", "it-IT"), ("پارسی", "fa-IR") };
    }
}
