namespace BlazorBoilerplate.Shared.Localizer
{
    public static class Settings
    {
        public const string NeutralCulture = "en-US";

        public static readonly string[] SupportedCultures = { NeutralCulture, "de-DE", "it-IT", "fa-IR", "pt-PT" };

        public static readonly (string, string)[] SupportedCulturesWithName = new[] { ("English", NeutralCulture), ("Deutsch", "de-DE"), ("Italiano", "it-IT"), ("پارسی", "fa-IR"), ("Português", "pt-PT") };
    }
}
