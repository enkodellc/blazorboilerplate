namespace BlazorBoilerplate.Localization
{
    public static class Settings
    {
        public static readonly string[] SupportedCultures = { "en-US", "it-IT", "fa-IR" };

        public static readonly (string, string)[] SupportedCulturesWithName = new[] { ("English", "en-US"), ("Italiano", "it-IT"), ("پارسی", "fa-IR") };
    }
}
