namespace BlazorBoilerplate.Shared.Interfaces.Db
{
    public interface ILocalizationRecord
    {
        long Id { get; set; }
        string Key { get; set; }
        string Text { get; set; }
        string LocalizationCulture { get; set; }
        string ResourceKey { get; set; }
    }
}
